using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                await MergeGuestCartAsync(model.Email);

                TempData["ToastMessage"] = "Welcome back!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account is locked out. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await MergeGuestCartAsync(model.Email);

                TempData["ToastMessage"] = "Account created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            TempData["ToastMessage"] = "You have been signed out.";
            TempData["ToastType"] = "info";
            return RedirectToAction("Index", "Home");
        }

        // Merges the guest session cart into the now-logged-in user's DB cart.
        // Matching products get their quantity combined instead of duplicated.
        private async Task MergeGuestCartAsync(string email)
        {
            var sessionId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(sessionId))
                return;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return;

            var guestItems = await _context.CartItems
                .Where(c => c.SessionId == sessionId)
                .ToListAsync();

            if (!guestItems.Any())
                return;

            foreach (var guestItem in guestItems)
            {
                var existing = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == guestItem.ProductId);

                if (existing != null)
                {
                    existing.Quantity += guestItem.Quantity;
                    _context.CartItems.Remove(guestItem);
                }
                else
                {
                    guestItem.UserId = user.Id;
                    guestItem.SessionId = null;
                }
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("CartSessionId");
        }
    }
}