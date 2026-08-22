using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    public class ModerationController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public ModerationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pendingComments = await _context.Comments
                .Include(c => c.Product)
                .Include(c => c.User)
                .Where(c => !c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new PendingCommentVM
                {
                    Id = c.Id,
                    ProductName = c.Product.Name,
                    UserFullName = c.User.FullName,
                    Rating = c.Rating,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var pendingTestimonials = await _context.Testimonials
                .Include(t => t.User)
                .Where(t => !t.IsApproved)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new PendingTestimonialVM
                {
                    Id = t.Id,
                    UserFullName = t.User.FullName,
                    Text = t.Text,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            var model = new ModerationVM
            {
                PendingComments = pendingComments,
                PendingTestimonials = pendingTestimonials
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                comment.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Comment approved";
                TempData["ToastType"] = "success";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Comment rejected";
                TempData["ToastType"] = "info";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTestimonial(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                testimonial.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Testimonial approved";
                TempData["ToastType"] = "success";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectTestimonial(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                _context.Testimonials.Remove(testimonial);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Testimonial rejected";
                TempData["ToastType"] = "info";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}