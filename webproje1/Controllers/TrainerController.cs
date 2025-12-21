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

        // ============================
        // DASHBOARD
        // ============================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var trainer = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.GymCenter)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
                return View("NoTrainerProfile");

            ViewBag.TotalAppointments = await _context.Appointments
                .CountAsync(a => a.TrainerId == trainer.Id);

            ViewBag.PendingAppointments = await _context.Appointments
                .CountAsync(a => a.TrainerId == trainer.Id && a.Status == AppointmentStatus.Pending);

            ViewBag.TodayAppointments = await _context.Appointments
                .CountAsync(a => a.TrainerId == trainer.Id &&
                                 a.AppointmentDate.Date == DateTime.Today);

            ViewBag.TrainerName = $"{trainer.User.FirstName} {trainer.User.LastName}";
            ViewBag.Specialization = trainer.Specialization;

            return View(trainer);
        }

        // ============================
        // PROFİL OLUŞTUR (GET)
        // ============================
        [HttpGet]
        public IActionResult CreateProfile()
        {
            return View();
        }

        // ============================
        // PROFİL OLUŞTUR (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(
            string Specialization,
            string Bio,
            int ExperienceYears,
            List<int> ServiceTypes)
        {
            var user = await _userManager.GetUserAsync(User);

            if (ServiceTypes == null || !ServiceTypes.Any())
            {
                TempData["Error"] = "En az bir hizmet seçmelisiniz.";
                return RedirectToAction(nameof(CreateProfile));
            }

            // Varsayılan salon
            var gym = await _context.GymCenters.FirstOrDefaultAsync();
            if (gym == null)
            {
                gym = new GymCenter
                {
                    Name = "FitZone Merkez",
                    Address = "Merkez",
                    PhoneNumber = "0000000000",
                    Email = "info@fitzone.com",
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0),
                    IsActive = true
                };
                _context.GymCenters.Add(gym);
                await _context.SaveChangesAsync();
            }

            var trainer = new Trainer
            {
                UserId = user.Id,
                GymCenterId = gym.Id,
                Specialization = Specialization,
                Bio = Bio,
                ExperienceYears = ExperienceYears,
                IsAvailable = true
            };

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            // Hizmetler
            foreach (var serviceTypeValue in ServiceTypes)
            {
                var serviceType = (ServiceType)serviceTypeValue;

                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceType == serviceType);

                if (service == null)
                {
                    service = new Service
                    {
                        Name = serviceType.ToString(),
                        ServiceType = serviceType,
                        GymCenterId = gym.Id,
                        Price = 100,
                        DurationMinutes = 60,
                        IsActive = true
                    };
                    _context.Services.Add(service);
                    await _context.SaveChangesAsync();
                }

                _context.TrainerServices.Add(new TrainerService
                {
                    TrainerId = trainer.Id,
                    ServiceId = service.Id
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Trainer profiliniz oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // MÜSAİTLİK LİSTESİ
        // ============================
        public async Task<IActionResult> Availability()
        {
            var user = await _userManager.GetUserAsync(User);

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
                return View("NoTrainerProfile");

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainer.Id)
                .OrderBy(a => a.AvailableDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return View(availabilities);
        }

        // ============================
        // MÜSAİTLİK EKLE (GET)
        // ============================
        public async Task<IActionResult> AddAvailability()
        {
            var user = await _userManager.GetUserAsync(User);

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
                return View("NoTrainerProfile");

            return View();
        }

        // ============================
        // MÜSAİTLİK EKLE (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(TrainerAvailability model)
        {
            var user = await _userManager.GetUserAsync(User);

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
                return View("NoTrainerProfile");

            bool isDuplicate = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == trainer.Id &&
                a.AvailableDate == model.AvailableDate &&
                a.StartTime == model.StartTime);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Bu tarih ve saat için zaten müsaitlik eklenmiş.");
                return View(model);
            }

            model.TrainerId = trainer.Id;
            model.IsActive = true;

            _context.TrainerAvailabilities.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Müsaitlik eklendi.";
            return RedirectToAction(nameof(Availability));
        }

        // ============================
        // MÜSAİTLİK SİL
        // ============================
        [HttpPost]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);

            if (availability != null)
            {
                _context.TrainerAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Availability));
        }

        // ============================
        // RANDEVULARIM
        // ============================
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
                return View("NoTrainerProfile");

            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Where(a => a.TrainerId == trainer.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // ============================
        // RANDEVU ONAYLA
        // ============================
        [HttpPost]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.ConfirmedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyAppointments));
        }

        // ============================
        // RANDEVU REDDET
        // ============================
        [HttpPost]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyAppointments));
        }

        // ============================
        // PROFİL GÖRÜNTÜLE
        // ============================
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            var trainer = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.GymCenter)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trainer == null)
                return View("NoTrainerProfile");

            return View(trainer);
        }
    }
}
