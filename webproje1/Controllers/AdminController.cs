using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webproje1.Models;

namespace webproje1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewBag.AdminEmail = User.Identity.Name;

            var users = _userManager.Users.ToList();

            ViewBag.TrainerIds = _userManager
                .GetUsersInRoleAsync("Trainer")
                .Result
                .Select(u => u.Id)
                .ToList();

            return View(users);
        }



        // İlerisi için hazır - Antrenör Yönetimi
        public IActionResult Trainers()
        {
            ViewBag.Message = "Antrenör yönetimi yakında eklenecek!";
            return View();
        }

        // İlerisi için hazır - Üye Yönetimi
        public IActionResult Members()
        {
            ViewBag.Message = "Üye yönetimi yakında eklenecek!";
            return View();
        }

        // İlerisi için hazır - Randevu Yönetimi
        public IActionResult Appointments()
        {
            ViewBag.Message = "Randevu yönetimi yakında eklenecek!";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> MakeTrainer(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Member"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Member");
                }

                if (!await _userManager.IsInRoleAsync(user, "Trainer"))
                {
                    await _userManager.AddToRoleAsync(user, "Trainer");
                }
            }

            return RedirectToAction("Index");
        }

    }
}