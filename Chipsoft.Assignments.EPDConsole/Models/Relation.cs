using Chipsoft.Assignments.EPDConsole.Enums;
using Chipsoft.Assignments.EPDConsole.Models;
using System.ComponentModel.DataAnnotations;

namespace Chipsoft.Assignments.EPDConsole.Model
{
    public class Relation
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string DisplayNumber { get; set; }

        [Required]
        [MaxLength(100)] 
        public string Name { get; set; }

        [Required]
        [MaxLength(100)] 
        public string FirstName { get; set; }

        [Required]
        public RelationType Type { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public Address Address { get; set; }

        public List<ContactOption> ContactOptions { get; set; }
        public List<Appointment> Appointments { get; set; }   
    }
}

