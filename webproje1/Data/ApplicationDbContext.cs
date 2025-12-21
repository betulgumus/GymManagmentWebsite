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

            // TrainerService çoka-çok ilişkisi (Composite Key)
            modelBuilder.Entity<TrainerService>()
                .HasKey(ts => new { ts.TrainerId, ts.ServiceId });

            // Trainer - User ilişkisi (One-to-One)
            modelBuilder.Entity<Trainer>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<Trainer>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // MemberProfile - User ilişkisi (One-to-One)
            modelBuilder.Entity<MemberProfile>()
                .HasOne(m => m.User)
                .WithOne()
                .HasForeignKey<MemberProfile>(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // NOT: Appointment ilişkileri SİLİNDİ! 
            // EF Core otomatik algılıyor (convention-based)
        }
    }
}