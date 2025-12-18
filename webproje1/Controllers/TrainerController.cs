using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webproje1.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.TrainerEmail = User.Identity.Name;
            return View();
        }
    }
}

