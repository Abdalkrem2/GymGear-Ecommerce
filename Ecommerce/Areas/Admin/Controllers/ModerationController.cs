using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Areas.Admin.Controllers
{
    public class ModerationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
