using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webproje1.Models;

namespace webproje1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler (Tablolar)
        public DbSet<GymCenter> GymCenters { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<MemberProfile> MemberProfiles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<TrainerService> TrainerServices { get; set; }
        public DbSet<AIRecommendation> AIRecommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tüm Foreign Key'lerde Cascade Delete'i kapat
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Appointment - Member ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Member)
                .WithMany()
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment - Trainer ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment - Service ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment - GymCenter ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.GymCenter)
                .WithMany()
                .HasForeignKey(a => a.GymCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // TrainerService çoka-çok ilişkisi
            modelBuilder.Entity<TrainerService>()
                .HasKey(ts => new { ts.TrainerId, ts.ServiceId });

            modelBuilder.Entity<TrainerService>()
                .HasOne(ts => ts.Trainer)
                .WithMany(t => t.TrainerServices)
                .HasForeignKey(ts => ts.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainerService>()
                .HasOne(ts => ts.Service)
                .WithMany(s => s.TrainerServices)
                .HasForeignKey(ts => ts.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trainer - User ilişkisi
            modelBuilder.Entity<Trainer>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<Trainer>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // MemberProfile - User ilişkisi
            modelBuilder.Entity<MemberProfile>()
                .HasOne(m => m.User)
                .WithOne()
                .HasForeignKey<MemberProfile>(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}