using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // TEMP (Phase 1): returns an empty HomeVM so the View doesn't break.
            // Belal replaces this with real category-fetching logic.
            return View(new HomeVM());
        }

        public IActionResult About()
        {
            // TEMP (Phase 1): returns an empty AboutVM so the View doesn't break.
            // Belal replaces this with real testimonial-fetching logic.
            return View(new AboutVM());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}