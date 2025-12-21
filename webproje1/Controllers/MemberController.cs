using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webproje1.Data;
using webproje1.Models;
using webproje1.ViewModels;

namespace webproje1.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(
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
            if (user == null)
                return RedirectToAction("Login", "Account");

            var profile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            ViewBag.MemberName = profile != null
                ? $"{profile.FirstName} {profile.LastName}"
                : user.Email;

            ViewBag.Email = user.Email;

            return View();
        }

        // ============================
        // 1️⃣ HİZMET SEÇ
        // ============================
        public async Task<IActionResult> SelectService()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View("SelectService", services);
        }

        // ============================
        // RANDEVU AL (HİZMET SEÇ)
        // ============================
        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View("BookAppointment", services);
        }

        // ============================
        // 2️⃣ ANTRENÖR SEÇ
        // ============================
        [HttpGet]
        public async Task<IActionResult> SelectTrainer(int serviceId)
        {
            var trainers = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.TrainerServices)
                .Where(t =>
                    t.IsAvailable &&
                    t.TrainerServices.Any(ts => ts.ServiceId == serviceId))
                .OrderBy(t => t.User.FirstName)
                .ToListAsync();

            ViewBag.ServiceId = serviceId;
            return View("SelectTrainer", trainers);
        }

        // ============================
        // 3️⃣ TARİH & SAAT SEÇ
        // ============================
        [HttpGet]
        public async Task<IActionResult> SelectDateTime(int trainerId, int serviceId)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);

            if (service == null)
                return RedirectToAction(nameof(SelectService));

            var availabilities = await _context.TrainerAvailabilities
                .Where(a =>
                    a.TrainerId == trainerId &&
                    a.IsActive &&
                    a.AvailableDate.Date >= DateTime.Today)
                .OrderBy(a => a.AvailableDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var existingAppointments = await _context.Appointments
                .Where(a =>
                    a.TrainerId == trainerId &&
                    a.AppointmentDate.Date >= DateTime.Today &&
                    a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            var duration = TimeSpan.FromMinutes(service.DurationMinutes);
            var slots = new List<TimeSlotViewModel>();

            foreach (var availability in availabilities)
            {
                var start = availability.StartTime;
                var latestStart = availability.EndTime - duration;

                while (start <= latestStart)
                {
                    var end = start + duration;

                    var overlap = existingAppointments.Any(a =>
                        a.AppointmentDate.Date == availability.AvailableDate.Date &&
                        start < a.EndTime &&
                        end > a.StartTime);

                    if (!overlap)
                    {
                        slots.Add(new TimeSlotViewModel
                        {
                            TrainerId = trainerId,
                            ServiceId = serviceId,
                            Date = availability.AvailableDate.Date,
                            StartTime = start,
                            EndTime = end
                        });
                    }

                    start = start.Add(duration);
                }
            }

            ViewBag.TrainerId = trainerId;
            ViewBag.ServiceId = serviceId;

            return View("SelectDateTime", slots);
        }

        // ============================
        // RANDEVU OLUŞTUR
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(
            int serviceId,
            int trainerId,
            DateTime availableDate,
            TimeSpan startTime)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);

            if (service == null)
                return RedirectToAction(nameof(SelectService));

            var duration = TimeSpan.FromMinutes(service.DurationMinutes);
            var endTime = startTime.Add(duration);

            var availability = await _context.TrainerAvailabilities
                .FirstOrDefaultAsync(a =>
                    a.TrainerId == trainerId &&
                    a.AvailableDate.Date == availableDate.Date &&
                    a.IsActive &&
                    startTime >= a.StartTime &&
                    endTime <= a.EndTime);

            if (availability == null)
            {
                TempData["Error"] = "Seçilen saat için müsaitlik yok.";
                return RedirectToAction(nameof(SelectDateTime), new { trainerId, serviceId });
            }

            var hasOverlap = await _context.Appointments.AnyAsync(a =>
                a.TrainerId == trainerId &&
                a.AppointmentDate.Date == availableDate.Date &&
                a.Status != AppointmentStatus.Cancelled &&
                startTime < a.EndTime &&
                endTime > a.StartTime);

            if (hasOverlap)
            {
                TempData["Error"] = "Bu saat için randevu mevcut.";
                return RedirectToAction(nameof(SelectDateTime), new { trainerId, serviceId });
            }

            var appointment = new Appointment
            {
                MemberId = user.Id,
                TrainerId = trainerId,
                ServiceId = serviceId,
                GymCenterId = service.GymCenterId,
                AppointmentDate = availableDate.Date,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = service.Price,
                Status = AppointmentStatus.Pending,
                CreatedDate = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Randevu başarıyla oluşturuldu.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // ============================
        // RANDEVULARIM
        // ============================
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.User)
                .Include(a => a.Service)
                .Include(a => a.GymCenter)
                .Where(a => a.MemberId == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }
    }
}
