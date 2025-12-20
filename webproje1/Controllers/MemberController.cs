using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webproje1.Data;
using webproje1.Models;

namespace webproje1.Controllers
{
    [Authorize(Roles = "Member")]
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

            var profile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            ViewBag.MemberName = $"{profile?.FirstName} {profile?.LastName}";
            ViewBag.Email = user.Email;

            return View("Index");
        }

        // ============================
        // 1️⃣ HİZMET SEÇ
        // ============================
        public async Task<IActionResult> SelectService()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .ToListAsync();

            return View("SelectService", services);
        }

        // ============================
        // 2️⃣ TRAINER SEÇ
        // ============================
        public async Task<IActionResult> SelectTrainer(int serviceId)
        {
            if (serviceId == 0)
                return RedirectToAction(nameof(SelectService));

            var trainers = await _context.TrainerServices
                .Include(ts => ts.Trainer)
                    .ThenInclude(t => t.User)
                .Where(ts => ts.ServiceId == serviceId)
                .Select(ts => ts.Trainer)
                .Distinct()
                .ToListAsync();

            ViewBag.ServiceId = serviceId;

            return View("SelectTrainer", trainers);
        }

        // ============================
        // 3️⃣ TARİH & SAAT SEÇ
        // ============================
        public async Task<IActionResult> SelectDateTime(int trainerId, int serviceId)
        {
            if (trainerId == 0 || serviceId == 0)
                return RedirectToAction(nameof(SelectService));

            var availabilities = await _context.TrainerAvailabilities
                .Where(a =>
                    a.TrainerId == trainerId &&
                    a.IsActive &&
                    a.AvailableDate >= DateTime.Today)
                .OrderBy(a => a.AvailableDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.TrainerId = trainerId;
            ViewBag.ServiceId = serviceId;

            return View("SelectDateTime", availabilities);
        }

        // ============================
        // 4️⃣ RANDEVU OLUŞTUR
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(
            int trainerId,
            int serviceId,
            DateTime availableDate,
            TimeSpan startTime)
        {
            var user = await _userManager.GetUserAsync(User);
            var service = await _context.Services.FindAsync(serviceId);

            if (service == null)
                return RedirectToAction(nameof(SelectService));

            var gym = await _context.GymCenters.FirstAsync();

            var appointment = new Appointment
            {
                MemberId = user.Id,
                TrainerId = trainerId,
                ServiceId = serviceId,
                GymCenterId = gym.Id,
                AppointmentDate = availableDate,
                StartTime = startTime,
                EndTime = startTime.Add(TimeSpan.FromMinutes(service.DurationMinutes)),
                TotalPrice = service.Price,
                Status = AppointmentStatus.Pending,
                CreatedDate = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Randevu talebiniz oluşturuldu!";
            return RedirectToAction(nameof(MyAppointments));
        }

        // ============================
        // RANDEVULARIM
        // ============================
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.User)
                .Include(a => a.Service)
                .Include(a => a.GymCenter)
                .Where(a => a.MemberId == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View("MyAppointments", appointments);
        }

        // ============================
        // PROFİL
        // ============================
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            var profile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            return View("Profile", profile);
        }

        // ============================
        // PROFİL DÜZENLE
        // ============================
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            var profile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            return View("EditProfile", profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(MemberProfile model)
        {
            var user = await _userManager.GetUserAsync(User);

            var profile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            profile.FirstName = model.FirstName;
            profile.LastName = model.LastName;
            profile.DateOfBirth = model.DateOfBirth;
            profile.Height = model.Height;
            profile.Weight = model.Weight;
            profile.BodyType = model.BodyType;
            profile.FitnessGoal = model.FitnessGoal;
            profile.HealthConditions = model.HealthConditions;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profil güncellendi.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
