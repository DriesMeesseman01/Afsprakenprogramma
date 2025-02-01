using Chipsoft.Assignments.EPDConsole.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chipsoft.Assignments.EPDConsole.Models
{
    public class Availability
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public WeekDay DayOfWeek { get; set; } 
        public List<TimeSlot> TimeSlots { get; set; }
        public Guid PhysicianId { get; set; }
        public Physician Physician { get; set; }
    }
}
