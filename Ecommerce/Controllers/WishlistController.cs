using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Ecommerce.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var vm = new WishlistVM
            {
                Items = await _context.WishlistItems
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Product)
                        .ThenInclude(p => p!.Category)
                    .Include(w => w.Product)
                        .ThenInclude(p => p!.Images)
                    .OrderByDescending(w => w.AddedAt)
                    .Select(w => new WishlistItemVM
                    {
                        WishlistItemId = w.Id,
                        ProductId = w.ProductId,
                        ProductName = w.Product != null ? w.Product.Name : string.Empty,
                        CategoryName = w.Product != null && w.Product.Category != null ? w.Product.Category.Name : string.Empty,
                        Price = w.Product != null ? w.Product.Price : 0,
                        MainImagePath = w.Product != null
                            ? (w.Product.Images.Where(i => i.IsMain).Select(i => i.ImagePath).FirstOrDefault()
                                ?? w.Product.Images.Select(i => i.ImagePath).FirstOrDefault())
                            : null
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        // POST: /Wishlist/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = _userManager.GetUserId(User);

            var alreadyExists = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            if (!alreadyExists)
            {
                _context.WishlistItems.Add(new WishlistItem
                {
                    UserId = userId!,
                    ProductId = productId,
                    AddedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Added to wishlist.";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "This item is already in your wishlist.";
                TempData["ToastType"] = "info";
            }

            // Redirect back to wherever the request came from (product page, storefront, etc.)
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int wishlistItemId)
        {
            var userId = _userManager.GetUserId(User);

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.Id == wishlistItemId && w.UserId == userId);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Removed from wishlist.";
                TempData["ToastType"] = "success";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}