using Chipsoft.Assignments.EPDConsole.Enums;
using Chipsoft.Assignments.EPDConsole.Models;

namespace Chipsoft.Assignments.EPDConsole.BLL
{
    public class PhysicianBuilder : RelationService
    {
        public PhysicianBuilder()
        {
            _relation = new Physician();
        }

        public void CreatePhysician()
        {
            base.CreateRelation();
            if (_relation is not Physician physician)
                throw new InvalidCastException("The relation is not a physician.");

            physician.Type = RelationType.PHYSICIAN;
            var availableDays = new List<Availability>();
            Console.WriteLine("Vul de beschikbaarheden in van de arts: ");

            while (true)
            {
                var availableDay = SelectAvailableDay(availableDays, physician.Id);

                while (true)
                {
                    var (startTime, endTime) = ReadStartAndEndTime();
                    availableDay.TimeSlots.Add(new TimeSlot
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        AvailabilityId = availableDay.Id,
                    });

                    Console.Write("Nog een tijd toevoegen? (ja/nee): ");
                    if (Console.ReadLine()?.ToLower() != "ja") break;
                }
                availableDays?.Add(availableDay);

                Console.Write("Nog een dag toevoegen? (ja/nee): ");
                if (Console.ReadLine()?.ToLower() != "ja") break;
            }
            if (availableDays?.Count() > 0)
            {
                physician.Availabilities = availableDays;
            }

            _dbContext.Add(physician);
            _dbContext.SaveChanges();
            Console.Write($"Arts is toegevoegd!");
            Console.ReadLine();
        }

        public Availability SelectAvailableDay(List<Availability> availableDays, Guid physicianId)
        {
            while (true)
            {
                Console.WriteLine("Selecteer een dag (1. Maandag, 2. Dinsdag, 3. Woensdag, 4. Donderdag, 5. Vrijdag)");
                if (int.TryParse(Console.ReadLine(), out int dayChoice) && dayChoice >= 1 && dayChoice <= 5)
                {
                    var selectedDay = (WeekDay)(dayChoice - 1);
                    return availableDays?.FirstOrDefault(x => x.DayOfWeek == selectedDay) ??
                                       new Availability
                                       {
                                           PhysicianId = physicianId,
                                           DayOfWeek = selectedDay,
                                           TimeSlots = new List<TimeSlot>()
                                       };
                }
            }
        }

        public (TimeSpan startTime, TimeSpan endTime) ReadStartAndEndTime()
        {
            TimeSpan startTime, endTime;

            startTime = ReadValidTime("Starttijd (HH:mm): ", TimeSpan.Zero, TimeSpan.FromDays(1));
            endTime = ReadValidTime("Eindtijd (HH:mm): ", startTime, TimeSpan.FromDays(1));

            return (startTime, endTime);
        }

        private TimeSpan ReadValidTime(string prompt, TimeSpan minTime, TimeSpan maxTime)
        {
            TimeSpan time;
            while (true)
            {
                Console.Write(prompt);
                if (TimeSpan.TryParse(Console.ReadLine(), out time) && time >= minTime && time < maxTime)
                    return time;

                Console.WriteLine("Ongeldige invoer. Zorg ervoor dat de tijd in het juiste formaat (HH:mm) is.");
                if (minTime != TimeSpan.Zero && time <= minTime)
                    Console.WriteLine("Eindtijd moet later zijn dan starttijd.");
            }
        }
    }
}
