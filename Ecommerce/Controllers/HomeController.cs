using System.Diagnostics;
using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using GymGear.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: / or /Home/Index
        public async Task<IActionResult> Index()
        {
            var vm = new HomeVM
            {
                Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryNavVM
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        // GET: /Home/About
        public async Task<IActionResult> About()
        {
            var vm = new AboutVM
            {
                Testimonials = await _context.Testimonials
                    .Where(t => t.IsApproved)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new TestimonialVM
                    {
                        UserFullName = t.User != null ? t.User.FullName : "Anonymous",
                        Text = t.Text,
                        CreatedAt = t.CreatedAt
                    })
                    .ToListAsync(),
                NewTestimonial = new TestimonialSubmitVM()
            };

            return View(vm);
        }

        // POST: /Home/SubmitTestimonial
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTestimonial(TestimonialSubmitVM model)
        {
            if (!ModelState.IsValid)
            {
                // Rebuild the full AboutVM so the view has its Testimonials list again,
                // with the invalid submission preserved for correction.
                var vm = new AboutVM
                {
                    Testimonials = await _context.Testimonials
                        .Where(t => t.IsApproved)
                        .OrderByDescending(t => t.CreatedAt)
                        .Select(t => new TestimonialVM
                        {
                            UserFullName = t.User != null ? t.User.FullName : "Anonymous",
                            Text = t.Text,
                            CreatedAt = t.CreatedAt
                        })
                        .ToListAsync(),
                    NewTestimonial = model
                };

                return View("About", vm);
            }

            var userId = _userManager.GetUserId(User);

            var testimonial = new Testimonial
            {
                UserId = userId!,
                Text = model.Text,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Thanks! Your testimonial was submitted and is awaiting approval.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(About));
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