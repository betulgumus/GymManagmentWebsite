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
            UserManager<ApplicationUser> userManager) //b
        {
            _context = context;
            _userManager = userManager;    
        }

        // Ana Sayfa
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            ViewBag.MemberName = $"{memberProfile?.FirstName} {memberProfile?.LastName}";
            ViewBag.Email = user.Email;

            return View();
        }

        // Antrenörler Listesi
        public async Task<IActionResult> Trainers()
        {
            var trainers = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.GymCenter)
                .Where(t => t.IsAvailable)
                .ToListAsync();

            return View(trainers);
        }

        // Hizmetler Listesi
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Include(s => s.GymCenter)
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(services);
        }

        // Randevularım
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

            return View(appointments);
        }

        // Randevu Al - GET
        public async Task<IActionResult> BookAppointment()
        {
            ViewBag.Trainers = await _context.Trainers
                .Include(t => t.User)
                .Where(t => t.IsAvailable)
                .ToListAsync();

            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive)
                .ToListAsync();



            return View();
        }

        // Randevu Al - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(Appointment model)
        {
            var user = await _userManager.GetUserAsync(User);

            // İlk (veya tek) GymCenter'ı otomatik al
            var defaultGymCenter = await _context.GymCenters.FirstOrDefaultAsync();

            if (defaultGymCenter == null)
            {
                TempData["Error"] = "Spor salonu bulunamadı!";
                return RedirectToAction("Index");
            }

            model.MemberId = user.Id;
            model.GymCenterId = defaultGymCenter.Id;  // ← Otomatik ata!
            model.Status = AppointmentStatus.Pending;
            model.CreatedDate = DateTime.Now;

            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevunuz başarıyla oluşturuldu!";
            return RedirectToAction("MyAppointments");
        }

        // Profilim
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            return View(memberProfile);
        }
        // Profil Düzenle - GET
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (memberProfile == null)
            {
                TempData["Error"] = "Profil bulunamadı!";
                return RedirectToAction("Index");
            }

            return View(memberProfile);
        }

        // Profil Düzenle - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(MemberProfile model)
        {
            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (memberProfile == null)
            {
                TempData["Error"] = "Profil bulunamadı!";
                return RedirectToAction("Index");
            }

            // Güncellenebilir alanlar
            memberProfile.FirstName = model.FirstName;
            memberProfile.LastName = model.LastName;
            memberProfile.DateOfBirth = model.DateOfBirth;
            memberProfile.Height = model.Height;
            memberProfile.Weight = model.Weight;
            memberProfile.BodyType = model.BodyType;
            memberProfile.FitnessGoal = model.FitnessGoal;
            memberProfile.HealthConditions = model.HealthConditions;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profiliniz başarıyla güncellendi!";
            return RedirectToAction("Profile");
        }
    }
}