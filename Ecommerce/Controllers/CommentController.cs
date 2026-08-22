using Ecommerce.Data;
using Ecommerce.Models.Entities;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Comment/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CommentSubmitVM model)
        {
            if (!ModelState.IsValid)
            {
                // Validation errors (bad rating range, empty text, etc.) — bounce back to the
                // product page. TempData carries the error so the view can surface it if it wants to.
                TempData["ToastMessage"] = "Please provide a valid rating and review text.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            var userId = _userManager.GetUserId(User);

            var comment = new Comment
            {
                ProductId = model.ProductId,
                UserId = userId!,
                Rating = model.Rating,
                Text = model.Text,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Thanks! Your review was submitted and is awaiting approval.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Details", "Product", new { id = model.ProductId });
        }
    }
}