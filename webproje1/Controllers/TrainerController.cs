using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webproje1.Data;
using webproje1.Models;

namespace webproje1.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Ana Sayfa
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.GymCenter)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            // İstatistikler
            var totalAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainer.Id)
                .CountAsync();

            var pendingAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainer.Id && a.Status == AppointmentStatus.Pending)
                .CountAsync();

            var todayAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainer.Id && a.AppointmentDate.Date == DateTime.Today)
                .CountAsync();

            ViewBag.TrainerName = $"{trainer.User.FirstName} {trainer.User.LastName}";
            ViewBag.Specialization = trainer.Specialization;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.PendingAppointments = pendingAppointments;
            ViewBag.TodayAppointments = todayAppointments;

            return View(trainer);
        }

        // Randevularım
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.GymCenter)
                .Where(a => a.TrainerId == trainer.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return View(appointments);
        }

        // Randevu Onayla
        [HttpPost]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.ConfirmedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu onaylandı!";
            }

            return RedirectToAction("MyAppointments");
        }

        // Randevu Reddet
        [HttpPost]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu reddedildi!";
            }

            return RedirectToAction("MyAppointments");
        }

        // Müsaitlik Ayarları
        public async Task<IActionResult> Availability()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainer.Id)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.TrainerId = trainer.Id;

            return View(availabilities);
        }

        // Müsaitlik Ekle - GET
        public async Task<IActionResult> AddAvailability()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            ViewBag.TrainerId = trainer.Id;
            return View();
        }

        // Müsaitlik Ekle - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(TrainerAvailability model)
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            model.TrainerId = trainer.Id;
            model.IsActive = true;

            _context.TrainerAvailabilities.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Müsaitlik başarıyla eklendi!";
            return RedirectToAction("Availability");
        }

        // Müsaitlik Sil
        [HttpPost]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);

            if (availability != null)
            {
                _context.TrainerAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Müsaitlik silindi!";
            }

            return RedirectToAction("Availability");
        }

        // Profilim
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.GymCenter)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            return View(trainer);
        }

        // Profil Düzenle - GET
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            return View(trainer);
        }

        // Profil Düzenle - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Trainer model)
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
            {
                return View("NoTrainerProfile");
            }

            // Güncellenebilir alanlar
            trainer.Specialization = model.Specialization;
            trainer.Bio = model.Bio;
            trainer.ExperienceYears = model.ExperienceYears;
            trainer.IsAvailable = model.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profiliniz başarıyla güncellendi!";
            return RedirectToAction("Profile");
        }
        // Profil Oluştur - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(string Specialization, string Bio, int ExperienceYears)
        {
            var user = await _userManager.GetUserAsync(User);

            // Zaten profil var mı kontrol et
            var existingTrainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (existingTrainer != null)
            {
                TempData["Error"] = "Zaten bir trainer profiliniz var!";
                return RedirectToAction("Index");
            }

            // İlk GymCenter'ı al (tek salon var)
            var defaultGym = await _context.GymCenters.FirstOrDefaultAsync();

            // Eğer GymCenter yoksa otomatik oluştur!
            if (defaultGym == null)
            {
                defaultGym = new GymCenter
                {
                    Name = "FitZone Merkez",
                    Address = "Serdivan, Sakarya",
                    PhoneNumber = "0264 123 4567",
                    Email = "info@fitzone.com",
                    OpeningTime = new TimeSpan(6, 0, 0),  // 06:00
                    ClosingTime = new TimeSpan(23, 0, 0), // 23:00
                    Description = "Ana spor salonumuz",
                    IsActive = true
                };

                _context.GymCenters.Add(defaultGym);
                await _context.SaveChangesAsync();
            }

            // Yeni Trainer profili oluştur
            var trainer = new Trainer
            {
                UserId = user.Id,
                GymCenterId = defaultGym.Id,
                Specialization = Specialization ?? "Genel Antrenör",
                Bio = Bio,
                ExperienceYears = ExperienceYears,
                IsAvailable = true,
                PhotoUrl = null
            };

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "🎉 Trainer profiliniz başarıyla oluşturuldu!";
            return RedirectToAction("Index");
        }
    }
}