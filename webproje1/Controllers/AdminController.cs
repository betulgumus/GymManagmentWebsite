using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using webproje1.Data;
using webproje1.Models;
using webproje1.ViewModels;

namespace webproje1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =======================
        // DASHBOARD
        // =======================
        public IActionResult Index()
        {
            ViewBag.AdminEmail = User.Identity?.Name;

            var users = _userManager.Users.ToList();

            ViewBag.TrainerIds = _userManager
                .GetUsersInRoleAsync("Trainer")
                .Result
                .Select(u => u.Id)
                .ToList();

            return View(users);
        }

        // =======================
        // ANTRÖNÖR YÖNETİMİ
        // =======================
        public async Task<IActionResult> Trainers()
        {
            var trainers = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .ToListAsync();

            return View(trainers);
        }

        public async Task<IActionResult> CreateTrainer()
        {
            var model = new AdminTrainerViewModel
            {
                AvailableUsers = await GetTrainerUserOptionsAsync(),
                AvailableServices = await GetServiceOptionsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrainer(AdminTrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await GetTrainerUserOptionsAsync();
                model.AvailableServices = await GetServiceOptionsAsync();
                return View(model);
            }

            if (await _context.Trainers.AnyAsync(t => t.UserId == model.UserId))
            {
                ModelState.AddModelError(nameof(model.UserId),
                    "Bu kullanıcı zaten antrenör olarak kayıtlı.");
                model.AvailableUsers = await GetTrainerUserOptionsAsync();
                model.AvailableServices = await GetServiceOptionsAsync();
                return View(model);
            }

            var gym = await GetOrCreateGymAsync();

            var trainer = new Trainer
            {
                UserId = model.UserId,
                GymCenterId = gym.Id,
                Specialization = model.Specialization,
                Bio = model.Bio,
                ExperienceYears = model.ExperienceYears,
                PhotoUrl = model.PhotoUrl,
                IsAvailable = model.IsAvailable
            };

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            await SyncTrainerServicesAsync(trainer.Id, model.SelectedServiceIds);

            TempData["Success"] = "Antrenör başarıyla eklendi.";
            return RedirectToAction(nameof(Trainers));
        }

        public async Task<IActionResult> EditTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            var model = new AdminTrainerViewModel
            {
                Id = trainer.Id,
                UserId = trainer.UserId,
                Specialization = trainer.Specialization,
                Bio = trainer.Bio,
                ExperienceYears = trainer.ExperienceYears,
                PhotoUrl = trainer.PhotoUrl,
                IsAvailable = trainer.IsAvailable,
                SelectedServiceIds = trainer.TrainerServices
                    .Select(ts => ts.ServiceId)
                    .ToList(),
                AvailableUsers = await GetTrainerUserOptionsAsync(),
                AvailableServices = await GetServiceOptionsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrainer(AdminTrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await GetTrainerUserOptionsAsync();
                model.AvailableServices = await GetServiceOptionsAsync();
                return View(model);
            }

            var trainer = await _context.Trainers.FindAsync(model.Id);
            if (trainer == null)
                return NotFound();

            trainer.UserId = model.UserId;
            trainer.Specialization = model.Specialization;
            trainer.Bio = model.Bio;
            trainer.ExperienceYears = model.ExperienceYears;
            trainer.PhotoUrl = model.PhotoUrl;
            trainer.IsAvailable = model.IsAvailable;

            await _context.SaveChangesAsync();
            await SyncTrainerServicesAsync(trainer.Id, model.SelectedServiceIds);

            TempData["Success"] = "Antrenör güncellendi.";
            return RedirectToAction(nameof(Trainers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer != null)
            {
                _context.TrainerServices.RemoveRange(trainer.TrainerServices);
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Antrenör silindi.";
            return RedirectToAction(nameof(Trainers));
        }

        // =======================
        // HİZMET YÖNETİMİ (404 ÇÖZÜLDÜ)
        // =======================
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Include(s => s.GymCenter)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(services);
        }

        public async Task<IActionResult> CreateService()
        {
            await GetOrCreateGymAsync();
            return View(new Service { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var gym = await GetOrCreateGymAsync();
            model.GymCenterId = gym.Id;

            _context.Services.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet eklendi.";
            return RedirectToAction(nameof(Services));
        }

        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(Service model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var service = await _context.Services.FindAsync(model.Id);
            if (service == null)
                return NotFound();

            service.Name = model.Name;
            service.Description = model.Description;
            service.DurationMinutes = model.DurationMinutes;
            service.Price = model.Price;
            service.ServiceType = model.ServiceType;
            service.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet güncellendi.";
            return RedirectToAction(nameof(Services));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Hizmet silindi.";
            return RedirectToAction(nameof(Services));
        }

        // =======================
        // ÜYE YÖNETİMİ
        // =======================
        public async Task<IActionResult> Members()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");
            return View(members);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = "Kullanıcı silindi.";
            }

            return RedirectToAction(nameof(Members));
        }

        // =======================
        // RANDEVU YÖNETİMİ
        // =======================
        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.User)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.ConfirmedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Appointments));
        }

        // =======================
        // HELPER METOTLAR
        // =======================
        private async Task<GymCenter> GetOrCreateGymAsync()
        {
            var gym = await _context.GymCenters.FirstOrDefaultAsync();
            if (gym != null) return gym;

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
            return gym;
        }

        private async Task<List<SelectListItem>> GetTrainerUserOptionsAsync()
        {
            return await _userManager.Users
                .OrderBy(u => u.Email)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.Email} ({u.FirstName} {u.LastName})"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetServiceOptionsAsync()
        {
            return await _context.Services
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        private async Task SyncTrainerServicesAsync(
            int trainerId,
            List<int>? selectedServiceIds)
        {
            selectedServiceIds ??= new List<int>();

            var existing = await _context.TrainerServices
                .Where(ts => ts.TrainerId == trainerId)
                .ToListAsync();

            var toRemove = existing
                .Where(ts => !selectedServiceIds.Contains(ts.ServiceId))
                .ToList();

            _context.TrainerServices.RemoveRange(toRemove);

            var existingIds = existing.Select(ts => ts.ServiceId).ToHashSet();

            var toAdd = selectedServiceIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new TrainerService
                {
                    TrainerId = trainerId,
                    ServiceId = id
                });

            await _context.TrainerServices.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
        }
    }
}
