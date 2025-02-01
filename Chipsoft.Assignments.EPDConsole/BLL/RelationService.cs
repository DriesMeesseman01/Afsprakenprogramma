using Chipsoft.Assignments.EPDConsole.BLL.Interfaces;
using Chipsoft.Assignments.EPDConsole.DAL;
using Chipsoft.Assignments.EPDConsole.Enums;
using Chipsoft.Assignments.EPDConsole.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Chipsoft.Assignments.EPDConsole.BLL
{
    public class RelationService : IRelationService
    {
        protected Relation _relation;
        protected EPDDbContext _dbContext;

        public RelationService()
        {
            _relation = new Relation();
            _dbContext = new EPDDbContext();
        }

        public RelationService CreateRelation()
        {
            _relation.Id = Guid.NewGuid();  
            _relation.DisplayNumber = _relation.Id.ToString("N").Substring(0, 8).ToUpper();
            _relation.Name = ReadInput("Vul de naam in: ");
            _relation.FirstName = ReadInput("Vul de voornaam in: ");
            _relation.BirthDate = ReadValidDate("\"Vul de geboortedatum in (yyyy-MM-dd): ");
            _relation.Gender = ReadValidGender();

            var address = new Address
            {
                City = ReadInput("Vul stad in: "),
                Municipalty = ReadInput("Vul gemeente in: "),
                Street = ReadInput("Vul straat in: "),
                PostalCode = ReadValidPostalCode("Vul postcode in: "),
                Country = ReadInput("Vul land in: ")
            };

            _relation.Address = address;
            _relation.ContactOptions = ReadContactOptions(_relation.Id);
            return this; 
        }

        public string ReadInput(string prompt, bool isRequired = true)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine()?.Trim() ?? string.Empty;
            } while (string.IsNullOrEmpty(input) && isRequired);

            return input;
        }

        public string ReadValidPostalCode(string prompt)
        {
            Regex postalCodeRegex = new Regex(@"^\d{4,6}$");
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine()?.Trim() ?? string.Empty;
            } while (!postalCodeRegex.IsMatch(input));

            return input;
        }

        public DateTime ReadValidDate(string prompt)
        {
            DateTime date;
            do
            {
                Console.Write(prompt);
            } while (!DateTime.TryParse(Console.ReadLine(), out date));

            return date;
        }

        public Gender ReadValidGender()
        {
            int choice;
            do
            {
                Console.WriteLine("Selecteer gender: ");
                Console.WriteLine("1. Man");
                Console.WriteLine("2. Vrouw");
                Console.WriteLine("3. Ander");
                Console.Write("Vul het nummer in dat bij het gender hoort: ");
            } while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 3);

            return (Gender)(choice - 1);
        }
        public ContactType ReadValidContactType()
        {
            Console.WriteLine("Welk contacttype wilt u toevoegen?");
            Console.WriteLine("1. Persoonlijke email");
            Console.WriteLine("2. Bedrijfs email");
            Console.WriteLine("3. Facturatie email");
            Console.WriteLine("4. Herinnerings email");
            Console.WriteLine("5. Persoonlijk telefoonnummer");
            Console.WriteLine("6. Bedrijf telefoonnummer");

            int choice;
            do
            {
                Console.Write("Vul het nummer in dat bij het contacttype hoort: ");
            } while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 6);

            return (ContactType)(choice - 1);
        }

        public List<ContactOption> ReadContactOptions(Guid relationId)
        {
            var contactOptions = new List<ContactOption>();
            bool addMore;

            do
            {
                var contactType = ReadValidContactType();
                string contactValue;

                if (contactType <= ContactType.REMINDEREMAIL)
                {
                    contactValue = ReadValidEmail();
                }
                else
                {
                    contactValue = ReadValidPhoneNumber();
                }

                contactOptions.Add(new ContactOption
                {
                    Type = contactType,
                    ContactValue = contactValue,
                    RelationId = relationId
                });

                Console.Write("Wil je een andere contactoptie toevoegen? (ja/nee): ");
                addMore = Console.ReadLine()?.Trim().ToLower() == "ja";

            } while (addMore);

            return contactOptions;
        }

        private string ReadValidEmail()
        {
            string email;
            Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$"); 

            do
            {
                Console.Write("Geef een geldig e-mailadres in: ");
                email = Console.ReadLine()?.Trim() ?? string.Empty;

                if (!emailRegex.IsMatch(email))
                {
                    Console.WriteLine("Ongeldig e-mailadres! Probeer opnieuw.");
                }

            } while (!emailRegex.IsMatch(email));

            return email;
        }

        private string ReadValidPhoneNumber()
        {
            string phone;
            Regex phoneRegex = new Regex(@"^\+?\d{8,15}$"); 

            do
            {
                Console.Write("Geef een geldig telefoonnummer in: ");
                phone = Console.ReadLine()?.Trim() ?? string.Empty;

                if (!phoneRegex.IsMatch(phone))
                {
                    Console.WriteLine("Ongeldig telefoonnummer! Voer een nummer in van 8-15 cijfers.");
                }

            } while (!phoneRegex.IsMatch(phone));

            return phone;
        }

        public void DeleteRelation(RelationType type)
        {
            var displayName = type == RelationType.PATIENT ? "patiënt" : "arts";
            var relations = GetAll(type);
            while (true)
            {
                var hasValues = ShowRelations(relations);
                if (!hasValues) return;

                Console.Write($"Vul het nummer in van de {displayName} die je wilt verwijderen: ");
                var relationNumber = Console.ReadLine()?.Trim();

                var relation = ValidateSelectedRelation(relationNumber, displayName);
                if (relation != null)
                {
                    _dbContext.Remove(relation);
                    _dbContext.SaveChanges();

                    Console.WriteLine($"{displayName} met nummer {relationNumber} is succesvol verwijderd.");
                    Console.ReadLine();
                    break;
                }
            }

        }

        public Relation? ValidateSelectedRelation(string? relationNumber, string displayName)
        {
            if (string.IsNullOrEmpty(relationNumber))
            {
                Console.WriteLine("Ongeldig nummer.");
                return null;  
            }

            var relation = _dbContext.Relations.FirstOrDefault(x => x.DisplayNumber.Equals(relationNumber));

            if (relation == null)
            {
                Console.WriteLine($"Geen {displayName} gevonden met nummer {relationNumber}.");
                return null; 
            }

            return relation;
        }


        public List<Relation> GetAll(RelationType type)
        {
            return _dbContext.Relations
                .Where(x => x.Type == type)
                .Include(x => x.Address)
                .ToList();
        }

        public bool ShowRelations(List<Relation> relations)
        {
            if (relations.Count == 0)
            {
                Console.WriteLine("Het systeem vond niets om weer te geven.");
                Console.ReadLine();
                return false;
            }

            Console.WriteLine("--------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-10} | {1,-20} | {2,-15} | {3,-15} | {4,-10} | {5,-20}",
                              "Nummer", "Naam", "Voornaam", "Stad", "Postcode", "Straat");
            Console.WriteLine("--------------------------------------------------------------------------------------");

            foreach (var relation in relations)
            {
                Console.WriteLine("{0,-10} | {1,-20} | {2,-15} | {3,-15} | {4,-10} | {5,-20}",
                    relation.DisplayNumber,
                    relation.Name ?? "N/A",
                    relation.FirstName ?? "N/A",
                    relation.Address?.City ?? "N/A",
                    relation.Address?.PostalCode ?? "N/A",
                    relation.Address?.Street ?? "N/A");
            }

            Console.WriteLine("--------------------------------------------------------------------------------------");
            return true;
        }
    }
}
