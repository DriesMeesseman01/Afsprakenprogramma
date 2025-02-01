using System.ComponentModel.DataAnnotations;

namespace Chipsoft.Assignments.EPDConsole.Model
{
    public class Address
    {
        [Key]
        [Required]
        public Guid Id { get; set; }    
        [Required]
        public string City { get; set; }
        [Required]
        public string Municipalty { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public Guid RelationId { get; set; } 
        public Relation Relation { get; set; } 
    }
}
