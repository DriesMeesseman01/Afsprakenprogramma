using Chipsoft.Assignments.EPDConsole.Enums;
using Chipsoft.Assignments.EPDConsole.Model;

namespace Chipsoft.Assignments.EPDConsole.BLL.Interfaces
{
    public interface IRelationService
    {
        public bool ShowRelations(List<Relation> relations);
        public List<Relation> GetAll(RelationType relationType);
        public Relation? ValidateSelectedRelation(string? relationNumber, string displayName);
        public string ReadInput(string prompt, bool isRequired = true);
    }
}
