using Ecommerce.Data;

using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Ecommerce.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Guests get a stable ID stored in their session; logged-in users use their real UserId.
        private string GetCartOwnerKey()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return _userManager.GetUserId(User);

            var sessionId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", sessionId);
            }
            return sessionId;
        }

        private bool IsLoggedIn => User.Identity != null && User.Identity.IsAuthenticated;

        public async Task<IActionResult> Index()
        {
            var ownerKey = GetCartOwnerKey();

            var query = _context.CartItems
                .Include(c => c.Product).ThenInclude(p => p.Category)
                .Include(c => c.Product).ThenInclude(p => p.Images)
                .AsQueryable();

            query = IsLoggedIn
                ? query.Where(c => c.UserId == ownerKey)
                : query.Where(c => c.SessionId == ownerKey);

            var items = await query.ToListAsync();

            var vm = new CartVM
            {
                Items = items.Select(c => new CartItemVM
                {
                    CartItemId = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    CategoryName = c.Product.Category.Name,
                    MainImagePath = c.Product.Images.FirstOrDefault()?.ImagePath,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity,
                    LineTotal = c.Product.Price * c.Quantity
                }).ToList()
            };
            vm.Subtotal = vm.Items.Sum(i => i.LineTotal);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var ownerKey = GetCartOwnerKey();

            var existing = IsLoggedIn
                ? await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == ownerKey && c.ProductId == productId)
                : await _context.CartItems.FirstOrDefaultAsync(c => c.SessionId == ownerKey && c.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = IsLoggedIn ? ownerKey : null,
                    SessionId = IsLoggedIn ? null : ownerKey
                });
            }

            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Added to cart";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                if (quantity <= 0)
                    _context.CartItems.Remove(item);
                else
                    item.Quantity = quantity;

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}