using Chipsoft.Assignments.EPDConsole.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chipsoft.Assignments.EPDConsole.Model
{
    public class ContactOption
    {
        [Required]
        public Guid Id { get; set; }    
        [Required]
        public ContactType Type { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContactValue { get; set; }
        public Guid RelationId { get; set; }
        public Relation Relation { get; set; }
    }
}
