//using Microsoft.AspNetCore.Identity;
//using webproje1.Models;

//namespace webproje1.Data
//{
//    public static class IdentitySeed
//    {
//        public static async Task SeedAsync(IServiceProvider serviceProvider)
//        {
//            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//            // Rolleri oluştur
//            string[] roles = { "Admin", "Member", "Trainer" };
//            foreach (var role in roles)
//            {
//                if (!await roleManager.RoleExistsAsync(role))
//                {
//                    await roleManager.CreateAsync(new IdentityRole(role));
//                }
//            }

//            // Admin kullanıcısı oluştur
//            string adminEmail = "ogrencinumarasi@sakarya.edu.tr";  // ← DÜZELTİLDİ!
//            string adminPassword = "sau";  // ← 6 karakter olmalı!

//            var adminUser = await userManager.FindByEmailAsync(adminEmail);
//            if (adminUser == null)
//            {
//                adminUser = new ApplicationUser
//                {
//                    UserName = adminEmail,
//                    Email = adminEmail,
//                    EmailConfirmed = true
//                };

//                var result = await userManager.CreateAsync(adminUser, adminPassword);
//                if (result.Succeeded)
//                {
//                    await userManager.AddToRoleAsync(adminUser, "Admin");
//                    Console.WriteLine("✅ Admin kullanıcısı oluşturuldu: " + adminEmail);
//                }
//                else
//                {
//                    Console.WriteLine("❌ Admin oluşturulamadı!");
//                    foreach (var error in result.Errors)
//                    {
//                        Console.WriteLine($"  - {error.Description}");
//                    }
//                }
//            }
//            else
//            {
//                Console.WriteLine("ℹ️ Admin zaten var!");
//            }
//        }
//    }
//}