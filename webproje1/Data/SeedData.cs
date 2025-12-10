using Microsoft.AspNetCore.Identity;
using webproje1.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace webproje1.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            IServiceProvider serviceProvider,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Veritabanını oluştur (yoksa)
            await context.Database.EnsureCreatedAsync();

            // Rolleri oluştur
            string[] roleNames = { "Admin", "Member", "Trainer" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Admin kullanıcısı oluştur
            var adminEmail = "ogrencinumarasi@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "sau");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Gym Center oluştur
            if (!context.GymCenters.Any())
            {
                var gymCenter = new GymCenter
                {
                    Name = "Merkez Fitness Salonu",
                    Address = "Sakarya, Türkiye",
                    Phone = "0264 123 45 67",
                    Email = "info@gymcenter.com",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(23, 0, 0)
                };
                context.GymCenters.Add(gymCenter);
                await context.SaveChangesAsync();

                // Hizmetler
                var services = new List<Service>
                {
                    new Service
                    {
                        Name = "Personal Training",
                        Description = "Birebir kişisel antrenman seansı",
                        Duration = 60,
                        Price = 300,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Yoga",
                        Description = "Grup yoga dersi",
                        Duration = 45,
                        Price = 150,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Pilates",
                        Description = "Grup pilates dersi",
                        Duration = 45,
                        Price = 150,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Crossfit",
                        Description = "Yoğun grup antrenman seansı",
                        Duration = 60,
                        Price = 200,
                        GymCenterId = gymCenter.Id
                    }
                };
                context.Services.AddRange(services);
                await context.SaveChangesAsync();

                // Örnek Antrenör
                var trainerEmail = "trainer@gym.com";
                var trainerUser = await userManager.FindByEmailAsync(trainerEmail);

                if (trainerUser == null)
                {
                    trainerUser = new ApplicationUser
                    {
                        UserName = trainerEmail,
                        Email = trainerEmail,
                        EmailConfirmed = true
                    };

                    var trainerResult = await userManager.CreateAsync(trainerUser, "Trainer123!");
                    if (trainerResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(trainerUser, "Trainer");

                        var trainer = new Trainer
                        {
                            UserId = trainerUser.Id,
                            Specialization = "Fitness & Strength Training",
                            Bio = "10 yıllık deneyime sahip profesyonel fitness antrenörü. Kas geliştirme ve kilo verme konusunda uzman.",
                            ExperienceYears = 10,
                            PhotoUrl = "/images/trainers/default-trainer.jpg",
                            GymCenterId = gymCenter.Id
                        };
                        context.Trainers.Add(trainer);
                        await context.SaveChangesAsync();

                        // Antrenör müsaitliği
                        // Antrenör müsaitliği
                        var days = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
                        foreach (var day in days)
                        {
                            context.TrainerAvailabilities.Add(new TrainerAvailability
                            {
                                TrainerId = trainer.Id,
                                DayOfWeek = (DayOfWeekEnum)(int)day,  // ← DÜZELTİLDİ! Cast eklendi
                                StartTime = new TimeSpan(9, 0, 0),
                                EndTime = new TimeSpan(17, 0, 0)
                            });
                        }
                        await context.SaveChangesAsync();

                        // Antrenör-Hizmet ilişkileri
                        context.TrainerServices.Add(new TrainerService
                        {
                            TrainerId = trainer.Id,
                            ServiceId = services[0].Id // Personal Training
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}

