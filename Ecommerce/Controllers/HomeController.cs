using System.Diagnostics;
using System.Globalization;
using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Models.Entities;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
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
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryNavVM
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            // 1. Fetch New Arrivals (Latest Active Products)
            var newArrivals = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Take(8)
                .Select(p => new ProductCardVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category != null ? p.Category.Name : "Gear",
                    MainImagePath = p.Images
                        .OrderByDescending(img => img.IsMain)
                        .ThenBy(img => img.Id)
                        .Select(img => img.ImagePath)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            // 2. Fetch Most Favorites (Highest Rated & Reviewed Products)
            var mostFavorites = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    Product = p,
                    AverageRating = p.Comments.Where(c => c.IsApproved).Select(c => (double?)c.Rating).Average() ?? 0,
                    ReviewCount = p.Comments.Count(c => c.IsApproved)
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewCount)
                .ThenByDescending(x => x.Product.Id)
                .Take(8)
                .Select(x => new ProductCardVM
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Price = x.Product.Price,
                    CategoryName = x.Product.Category != null ? x.Product.Category.Name : "Gear",
                    MainImagePath = x.Product.Images
                        .OrderByDescending(img => img.IsMain)
                        .ThenBy(img => img.Id)
                        .Select(img => img.ImagePath)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            var vm = new HomeVM
            {
                Categories = categories,
                NewArrivals = newArrivals,
                MostFavorites = mostFavorites
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
        public async Task<IActionResult> SubmitTestimonial(TestimonialSubmitVM NewTestimonial)
        {
            if (!ModelState.IsValid)
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
                    NewTestimonial = NewTestimonial
                };

                return View("About", vm);
            }

            var userId = _userManager.GetUserId(User);

            var testimonial = new Testimonial
            {
                UserId = userId!,
                Text = NewTestimonial.Text,
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

        [Route("Home/HandleError/{code:int}")]
        public IActionResult HandleError(int code)
        {
            if (code == 404)
            {
                return View("NotFound");
            }

            return View("Error");
        }
    }
}
