using Chipsoft.Assignments.EPDConsole.Enums;
using Chipsoft.Assignments.EPDConsole.Model;

namespace Chipsoft.Assignments.EPDConsole.BLL
{
    public class PatientBuilder : RelationService
    {
        public PatientBuilder()
        {
            _relation = new Patient();
        }

        public void CreatePatient()
        {
            base.CreateRelation();
            if (_relation is not Patient patient)
                throw new InvalidCastException("The relation is not a patient.");

            patient.Type = RelationType.PATIENT;
            patient.PolisNumber = ReadPolisNumber("Vul het polisnummer in (niet verplicht): ");
            patient.InsuranceCompany = base.ReadInput("Vul de verzekeringsmaatschappij in (niet verplicht): ", false);

            _dbContext.Add(patient);
            _dbContext.SaveChanges();
            Console.Write($"Patient is toegevoegd!");
            Console.ReadLine();
        }

        public string ReadPolisNumber(string prompt)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine()?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(input))
                {
                    return "";
                }

                if (!IsValidPolisNumber(input))
                {
                    Console.WriteLine("Ongeldig polisnummer. Het polisnummer moet 10 cijfers bevatten.");
                    continue;
                }

            } while (string.IsNullOrEmpty(input) || !IsValidPolisNumber(input));

            return input;
        }

        private bool IsValidPolisNumber(string polisNumber)
        {
            return polisNumber.Length == 10 && polisNumber.All(char.IsDigit);
        }
    }
}
