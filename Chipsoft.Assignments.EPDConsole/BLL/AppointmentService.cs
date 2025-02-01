using Chipsoft.Assignments.EPDConsole.BLL.Interfaces;
using Chipsoft.Assignments.EPDConsole.DAL;
using Chipsoft.Assignments.EPDConsole.Enums;
using Chipsoft.Assignments.EPDConsole.Model;
using Chipsoft.Assignments.EPDConsole.Models;
using Microsoft.EntityFrameworkCore;

namespace Chipsoft.Assignments.EPDConsole.BLL
{
    public class AppointmentService : IAppointmentService
    {
        private IRelationService _relationService;
        private EPDDbContext _dbContext;
        public AppointmentService(IRelationService relationBuilderService)
        {
            _relationService = relationBuilderService;
            _dbContext = new EPDDbContext();
        }
        public void CreateAppointment()
        {
            bool addApointment = true;
            while (addApointment)
            {
                var appointment = new Appointment();
                appointment.PatientId = GetPatientId();
                if (appointment.PatientId.Equals(Guid.Empty)) return;
                appointment.Date = GetAppointmentDate();

                var dayOfWeekMapping = new Dictionary<DayOfWeek, WeekDay>
                {
                    { DayOfWeek.Monday, WeekDay.MONDAY },
                    { DayOfWeek.Tuesday, WeekDay.TUESDAY },
                    { DayOfWeek.Wednesday, WeekDay.WEDNESDAY },
                    { DayOfWeek.Thursday, WeekDay.THURSDAY },
                    { DayOfWeek.Friday, WeekDay.FRIDAY }
                };

                var chosenDay = dayOfWeekMapping.ContainsKey(appointment.Date.DayOfWeek)
                    ? dayOfWeekMapping[appointment.Date.DayOfWeek]
                    : WeekDay.MONDAY;


                var availablePhysicians = _dbContext.Relations
                       .Include(x => x.Address)
                       .OfType<Physician>()
                       .Where(x => x.Availabilities
                       .Any(a => a.DayOfWeek.Equals(chosenDay)))
                       .OfType<Relation>()
                       .ToList();

                if (availablePhysicians.Count == 0)
                {
                    Console.WriteLine("Er zijn op deze datum geen beschikbare artsen, geef een andere datum in");
                } else
                {
                    appointment.PhysicianId = GetPhysicianId(availablePhysicians);
                    if (appointment.PhysicianId.Equals(Guid.Empty)) return;

                    var availableHours = _dbContext.Relations
                        .OfType<Physician>()
                        .Include(x => x.Availabilities)
                            .ThenInclude(a => a.TimeSlots)
                        .Where(x => x.Id.Equals(appointment.PhysicianId) && x.Availabilities
                            .Any(a => a.DayOfWeek == chosenDay))
                        .SelectMany(x => x.Availabilities)
                        .Where(a => a.DayOfWeek == chosenDay)
                        .SelectMany(a => a.TimeSlots)
                        .ToList();

                    var existingAppointments = _dbContext.Relations
                        .OfType<Physician>()
                        .Include(x => x.Appointments)
                        .ThenInclude(a => a.TimeSlot)
                        .Where(x => x.Id == appointment.PhysicianId &&
                             x.Appointments.Any(a => a.Date.Date == appointment.Date.Date)) 
                        .SelectMany(x => x.Appointments) 
                        .Select(a => a.TimeSlot)
                        .ToList();

                    var freeTimeSlots = GetFreeTimeSlots(availableHours, existingAppointments);

                    var timeSlot = GetValidTimeSlot(freeTimeSlots, appointment.Id);

                    _dbContext.Add(timeSlot);
                    appointment.TimeSlot = timeSlot;
                    _dbContext.Add(appointment);
                    _dbContext.SaveChanges();
                    addApointment = false;
                }
            }
            Console.Write($"Afspraak is toegevoegd!");
            Console.ReadLine();
        }

        public Guid GetPatientId()
        {
            while (true)
            {
                var hasValues = _relationService.ShowRelations(_relationService.GetAll(RelationType.PATIENT));
                if (!hasValues) return Guid.Empty;
                Console.Write("Vul het nummer in van de patient waarvoor je een afspraak wilt inplannen: ");
                var patientNumber = Console.ReadLine()?.Trim();
                var patient = _relationService.ValidateSelectedRelation(patientNumber, "patient");
                if (patient != null)
                {
                    return patient.Id;
                }
            }
        }

        public Guid GetPhysicianId(List<Relation> availablePhysicians)
        {
            while (true)
            {
                var hasValues = _relationService.ShowRelations(availablePhysicians);
                if (!hasValues) return Guid.Empty;

                Console.Write("Vul het nummer in van één van de beschikbare artsen: ");
                var physicianNumber = Console.ReadLine() ?? string.Empty;
                var physician = _relationService.ValidateSelectedRelation(physicianNumber, "arts");
                if (physician != null) {
                    return physician.Id;
                }
            }
        }

        public DateTime GetAppointmentDate()
        {
            DateTime appointmentDate;
            while (true)
            {
                Console.Write("Vul de datum van de afspraak in (yyyy-MM-dd): ");

                if (DateTime.TryParse(Console.ReadLine(), out appointmentDate) &&
                    appointmentDate > DateTime.Now &&  
                    appointmentDate.DayOfWeek != DayOfWeek.Saturday &&
                    appointmentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    return appointmentDate;
                }

                Console.WriteLine("De datum moet in de toekomst zijn en mag niet in het weekend vallen.");
            }
        }

        public List<TimeSlot> GetFreeTimeSlots(List<TimeSlot> availableHours, List<TimeSlot> existingAppointments)
        {
            var sortedAvailableHours = availableHours.OrderBy(a => a.StartTime).ToList();
            var sortedExistingAppointments = existingAppointments.OrderBy(e => e.StartTime).ToList();
            var freeTimeSlots = new List<TimeSlot>();

            foreach (var available in sortedAvailableHours)
            {
                var availableStart = available.StartTime;
                var availableEnd = available.EndTime;

                foreach (var existing in sortedExistingAppointments
                    .Where(e => e.StartTime < availableEnd && e.EndTime > availableStart))
                {
                    if (availableStart < existing.StartTime)
                    {
                        freeTimeSlots.Add(new TimeSlot { StartTime = availableStart, EndTime = existing.StartTime });
                    }

                    availableStart = existing.EndTime;
                }

                if (availableStart < availableEnd)
                {
                    freeTimeSlots.Add(new TimeSlot { StartTime = availableStart, EndTime = availableEnd });
                }
            }

            return freeTimeSlots;
        }

        public TimeSlot GetValidTimeSlot(List<TimeSlot> freeTimeSlots, Guid appointmentId)
        {
            TimeSpan startTime, endTime;

            while (true)
            {
                Console.WriteLine("Beschikbare werkuren arts:");
                freeTimeSlots.ForEach(slot => Console.WriteLine(slot.ToString()));

                Console.Write("Starttijd (HH:mm): ");
                if (!TimeSpan.TryParse(Console.ReadLine(), out startTime) 
                    || startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1))
                {
                    Console.WriteLine("Ongeldige starttijd. Zorg ervoor dat de tijd in het juiste formaat (HH:mm) is.");
                    continue;
                }

                Console.Write("Eindtijd (HH:mm): ");
                if (!TimeSpan.TryParse(Console.ReadLine(), out endTime) 
                    || endTime < TimeSpan.Zero || endTime >= TimeSpan.FromDays(1))
                {
                    Console.WriteLine("Ongeldige eindtijd. Zorg ervoor dat de tijd in het juiste formaat (HH:mm) is.");
                    continue;
                }

                if (endTime <= startTime)
                {
                    Console.WriteLine("Eindtijd moet later zijn dan starttijd.");
                    continue;
                }

                var isValidSlot = freeTimeSlots.Any(slot =>
                    slot.StartTime <= startTime && slot.EndTime >= endTime);

                if (isValidSlot)
                {
                    return new TimeSlot
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        AppointmentId = appointmentId
                    };
                }
                else
                {
                    Console.WriteLine("De ingevoerde tijden vallen niet binnen de beschikbare tijdsloten.");
                }
            }
        }

        public void ShowAppointment()
        {
            string input;

            while (true)
            {
               input = _relationService.ReadInput("Wil je de afspraken zien van een patient of arts? ");
               if (input.ToLower().Trim().Equals("patient") || input.ToLower().Trim().Equals("arts")) break;
                Console.WriteLine("Foute input, de geldige waardes zijn patient of arts");
            }

            var trimmedInput = input.ToLower().Trim();  
            var type = input.ToLower().Trim().Equals("patient") ? RelationType.PATIENT : RelationType.PHYSICIAN;
            var hasValues = _relationService.ShowRelations(_relationService.GetAll(type));
            if (!hasValues) return;

            var selectedRelation = new Relation();
            while (true)
            {
                var number = _relationService.ReadInput("Vul het nummer in van de "
                       + trimmedInput + " waarvan je de afspraken wilt zien. ");

                selectedRelation = _relationService.ValidateSelectedRelation(number, trimmedInput);
                if (selectedRelation != null) break;
            }

            var appointments = new List<Appointment>();
            if (type == RelationType.PATIENT) {
                appointments = _dbContext.Appointments.
                    Include(x => x.TimeSlot)
                   .Include(x => x.Physician)
                   .Where(x => x.PatientId.Equals(selectedRelation.Id))
                   .ToList();
            } 
            else
            {
                appointments = _dbContext.Appointments
                    .Include(x => x.TimeSlot)
                    .Include(x => x.Patient)
                    .Where(x => x.PhysicianId.Equals(selectedRelation.Id))
                    .ToList();
            }

            ShowAppointments(appointments, type);
        }

        public void ShowAppointments(List<Appointment> appointments, RelationType relationType)
        {
            if (appointments.Count == 0)
            {
                Console.WriteLine("Het systeem vond niets om weer te geven.");
                Console.ReadLine();
                return;
            }

            var sortedAppointments = appointments
                .OrderBy(a => a.Date)
                .ThenBy(a => a.TimeSlot.StartTime)
                .ToList();

            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("{0,-12} | {1,-8} | {2,-8} | {3,-20}",
                              "Datum", "Start", "Eind", relationType == RelationType.PATIENT ? "Dokter" : "Patiënt");
            Console.WriteLine("--------------------------------------------------------------");

            foreach (var appointment in sortedAppointments)
            {
                Console.WriteLine("{0,-12} | {1,-8} | {2,-8} | {3,-20}",
                    appointment.Date.ToString("yyyy-MM-dd"),
                    appointment.TimeSlot.StartTime.ToString(@"hh\:mm"),
                    appointment.TimeSlot.EndTime.ToString(@"hh\:mm"),
                    relationType == RelationType.PATIENT
                        ? $"{appointment.Physician.FirstName} {appointment.Physician.Name}"
                        : $"{appointment.Patient.FirstName} {appointment.Patient.Name}");
            }

            Console.WriteLine("--------------------------------------------------------------");
            Console.ReadLine();
        }
    }
}
