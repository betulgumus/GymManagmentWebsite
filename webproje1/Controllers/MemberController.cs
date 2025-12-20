using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webproje1.Data;
using webproje1.Models;
using webproje1.Services;
using webproje1.ViewModels;

namespace webproje1.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GroqChatService _groqChatService;

        public MemberController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            GroqChatService groqChatService)
        {
            _context = context;
            _userManager = userManager;
            _groqChatService = groqChatService;
        }

        // ============================
        // DASHBOARD
        // ============================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

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
        // RANDEVU OLUŞTUR
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(int serviceId, int trainerId, DateTime appointmentDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var gym = await _context.GymCenters.FirstOrDefaultAsync();
            if (gym == null)
            {
                ModelState.AddModelError(string.Empty, "Spor salonu bulunamadı.");
                return RedirectToAction(nameof(SelectService));
            }

            var appointment = new Appointment
            {
                MemberId = user.Id,
                TrainerId = trainerId,
                ServiceId = serviceId,
                GymCenterId = gym.Id,
                AppointmentDate = appointmentDate,
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

            return View("MyAppointments", appointments);
        }

        // ============================
        // ANTRENÖRLER
        // ============================
        public async Task<IActionResult> Trainers()
        {
            var trainers = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Where(t => t.IsAvailable)
                .ToListAsync();

            return View("Trainers", trainers);
        }

        // ============================
        // HİZMETLER
        // ============================
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View("Services", services);
        }

        // ============================
        // 🤖 AI ÖNERİLERİ
        // ============================
        public async Task<IActionResult> AiRecommendations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var history = await _context.AIRecommendations
                .Where(r => r.MemberId == user.Id)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToListAsync();

            var model = new AIRecommendationViewModel
            {
                History = history
            };

            return View("AiRecommendations", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AiRecommendations(
            AIRecommendationViewModel model,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.History = await _context.AIRecommendations
                    .Where(r => r.MemberId == user.Id)
                    .OrderByDescending(r => r.RequestDate)
                    .Take(5)
                    .ToListAsync();

                return View("AiRecommendations", model);
            }

            string response;
            var prompt = $"Soru: {model.Question}\nİstek Türü: {model.RequestType}";

            try
            {
                response = await _groqChatService
                    .GetRecommendationAsync(prompt, cancellationToken);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"AI servisine ulaşılamadı: {ex.Message}");

                model.History = await _context.AIRecommendations
                    .Where(r => r.MemberId == user.Id)
                    .OrderByDescending(r => r.RequestDate)
                    .Take(5)
                    .ToListAsync();

                return View("AiRecommendations", model);
            }

            var aiRecord = new AIRecommendation
            {
                MemberId = user.Id,
                RequestType = model.RequestType,
                InputData = model.Question,
                AIResponse = response,
                RequestDate = DateTime.Now
            };

            _context.AIRecommendations.Add(aiRecord);
            await _context.SaveChangesAsync();

            model.Response = response;
            model.History = await _context.AIRecommendations
                .Where(r => r.MemberId == user.Id)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToListAsync();

            return View("AiRecommendations", model);
        }

        // ============================
        // PROFİL
        // ============================
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

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
            if (user == null)
                return RedirectToAction("Login", "Account");

            var profile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            return View("EditProfile", profile);
        }
    }
}
