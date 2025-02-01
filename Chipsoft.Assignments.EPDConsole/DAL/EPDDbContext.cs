using Chipsoft.Assignments.EPDConsole.Model;
using Chipsoft.Assignments.EPDConsole.Models;
using Microsoft.EntityFrameworkCore;

namespace Chipsoft.Assignments.EPDConsole.DAL
{
    public class EPDDbContext : DbContext
    {
        // The following configures EF to create a Sqlite database file in the
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source=epd.db");
        public DbSet<Relation> Relations { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ContactOption> ContactOptions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Relation>()
                .HasOne(r => r.Address)
                .WithOne(a => a.Relation)
                .HasForeignKey<Address>(a => a.RelationId) 
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Relation>()
               .HasMany(p => p.ContactOptions)
               .WithOne(m => m.Relation)
               .HasForeignKey(m => m.RelationId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Physician>()
               .HasMany(p => p.Availabilities)
               .WithOne(m => m.Physician)
               .HasForeignKey(m => m.PhysicianId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Availability>()
               .HasMany(a => a.TimeSlots)
               .WithOne(ts => ts.Availability)
               .HasForeignKey(ts => ts.AvailabilityId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);  

            modelBuilder.Entity<Appointment>()
               .HasOne(a => a.Patient)
               .WithMany(p => p.Appointments)
               .HasForeignKey(p => p.PatientId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
               .HasOne(a => a.Physician)
               .WithMany(p => p.Appointments)
               .HasForeignKey(p => p.PhysicianId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeSlot>()
               .HasOne(a => a.Appointment)
               .WithOne(p => p.TimeSlot)
               .HasForeignKey<TimeSlot>(p => p.AppointmentId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
