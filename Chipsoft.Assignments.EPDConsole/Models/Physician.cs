using Chipsoft.Assignments.EPDConsole.Model;

namespace Chipsoft.Assignments.EPDConsole.Models
{
    public class Physician : Relation
    {
        public List<Availability> Availabilities { get; set; }
    }
}
