using Chipsoft.Assignments.EPDConsole.Model;
using System.ComponentModel.DataAnnotations;

namespace Chipsoft.Assignments.EPDConsole.Models
{
    public class Appointment
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public TimeSlot TimeSlot { get; set; }
        [Required]
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }
        [Required]
        public Guid PhysicianId { get; set; }
        public Physician Physician { get; set; }

    }
}
