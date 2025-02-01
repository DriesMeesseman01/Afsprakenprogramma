using Chipsoft.Assignments.EPDConsole.BLL;
using Chipsoft.Assignments.EPDConsole.BLL.Interfaces;
using Chipsoft.Assignments.EPDConsole.DAL;
using Chipsoft.Assignments.EPDConsole.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chipsoft.Assignments.EPDConsole
{
    public class Program
    {
        private static IServiceProvider _serviceProvider;
        private static T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        private static void AddPatient()
        {
            new PatientBuilder().CreatePatient();
        }

        private static void DeletePatient()
        {
            new RelationService().DeleteRelation(RelationType.PATIENT);
        }

        private static void AddPhysician()
        {
            new PhysicianBuilder().CreatePhysician();
        }

        private static void DeletePhysician()
        {
            new RelationService().DeleteRelation(RelationType.PHYSICIAN);
        }

        private static void AddAppointment()
        {
            GetService<IAppointmentService>().CreateAppointment();
        }

        private static void ShowAppointment()
        {
            GetService<IAppointmentService>().ShowAppointment();
        }

        #region FreeCodeForAssignment
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            _serviceProvider = host.Services;

            // Start the main menu loop
            while (ShowMenu())
            {
                // Continue
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                   
                    services.AddSingleton<IAppointmentService, AppointmentService>();
                    services.AddSingleton<IRelationService, RelationService>();

                    services.AddLogging(builder =>
                    {
                        builder.ClearProviders();  
                        builder.AddConsole();  
                    });
                });

        public static bool ShowMenu()
        {
            Console.Clear();
            foreach (var line in File.ReadAllLines("logo.txt"))
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("");
            Console.WriteLine("1 - Patient toevoegen");
            Console.WriteLine("2 - Patient verwijderen");
            Console.WriteLine("3 - Arts toevoegen");
            Console.WriteLine("4 - Arts verwijderen");
            Console.WriteLine("5 - Afspraak toevoegen");
            Console.WriteLine("6 - Afspraken inzien");
            Console.WriteLine("7 - Sluiten");
            Console.WriteLine("8 - Reset db");

            if (int.TryParse(Console.ReadLine(), out int option))
            {
                switch (option)
                {
                    case 1:
                        AddPatient();
                        return true;
                    case 2:
                        DeletePatient();
                        return true;
                    case 3:
                        AddPhysician();
                        return true;
                    case 4:
                        DeletePhysician();
                        return true;
                    case 5:
                        AddAppointment();
                        return true;
                    case 6:
                        ShowAppointment();
                        return true;
                    case 7:
                        return false;
                    case 8:
                        EPDDbContext dbContext = new EPDDbContext();
                        dbContext.Database.EnsureDeleted();
                        dbContext.Database.EnsureCreated();
                        return true;
                    default:
                        return true;
                }
            }
            return true;
        }

        #endregion
    }
}
