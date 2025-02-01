using System.ComponentModel.DataAnnotations;

namespace Chipsoft.Assignments.EPDConsole.Models
{
    public class TimeSlot
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }  
        [Required]
        public TimeSpan EndTime { get; set; }  
        public Guid? AvailabilityId { get; set; }  
        public Availability Availability { get; set; }
        public Guid? AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        public override string ToString()
        {
            return $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        }

    }
}
