using Ecommerce.Data;
using Ecommerce.Models.Entities;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private const int MaxQuantityPerProduct = 5;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCartOwnerKey()
        {
            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                return _userManager.GetUserId(User)
                    ?? string.Empty;
            }

            var sessionId =
                HttpContext.Session.GetString("CartSessionId");

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();

                HttpContext.Session.SetString(
                    "CartSessionId",
                    sessionId);
            }

            return sessionId;
        }

        private bool IsLoggedIn =>
            User.Identity != null &&
            User.Identity.IsAuthenticated;

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var ownerKey = GetCartOwnerKey();

            var query = _context.CartItems
                .AsNoTracking()
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Images)
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

                    ProductName =
                        c.Product?.Name ?? "Product unavailable",

                    CategoryName =
                        c.Product?.Category?.Name ?? string.Empty,

                    MainImagePath = c.Product?.Images
                        .OrderByDescending(image => image.IsMain)
                        .ThenBy(image => image.Id)
                        .Select(image => image.ImagePath)
                        .FirstOrDefault(),

                    UnitPrice = c.Product?.Price ?? 0,
                    Quantity = c.Quantity,

                    LineTotal =
                        (c.Product?.Price ?? 0) * c.Quantity
                }).ToList()
            };

            vm.Subtotal = vm.Items.Sum(i => i.LineTotal);

            return View(vm);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int productId,
            int quantity = 1)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            if (product.Stock <= 0)
            {
                TempData["ToastMessage"] =
                    "This product is out of stock.";

                TempData["ToastType"] = "error";

                return RedirectToAction(
                    "Details",
                    "Product",
                    new { id = productId });
            }

            var allowedQuantity = Math.Min(
                MaxQuantityPerProduct,
                product.Stock);

            var ownerKey = GetCartOwnerKey();

            var existing = IsLoggedIn
                ? await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.UserId == ownerKey &&
                        c.ProductId == productId)
                : await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.SessionId == ownerKey &&
                        c.ProductId == productId);

            var currentQuantity = existing?.Quantity ?? 0;
            var requestedTotal = currentQuantity + quantity;

            if (requestedTotal > allowedQuantity)
            {
                TempData["ToastMessage"] =
                    $"Maximum allowed quantity is {allowedQuantity}.";

                TempData["ToastType"] = "error";

                return RedirectToAction(nameof(Index));
            }

            if (existing != null)
            {
                existing.Quantity = requestedTotal;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,

                    UserId = IsLoggedIn
                        ? ownerKey
                        : null,

                    SessionId = IsLoggedIn
                        ? null
                        : ownerKey
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Added to cart";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(
            int cartItemId,
            int quantity)
        {
            var ownerKey = GetCartOwnerKey();

            var item = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c =>
                    c.Id == cartItemId &&
                    (IsLoggedIn
                        ? c.UserId == ownerKey
                        : c.SessionId == ownerKey));

            if (item == null)
            {
                return NotFound();
            }

            // Quantity zero removes the item.
            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] =
                    "Item removed from cart";

                TempData["ToastType"] = "success";

                return RedirectToAction(nameof(Index));
            }

            if (item.Product == null)
            {
                return NotFound();
            }

            var allowedQuantity = Math.Min(
                MaxQuantityPerProduct,
                item.Product.Stock);

            if (quantity > allowedQuantity)
            {
                TempData["ToastMessage"] =
                    $"Maximum allowed quantity is {allowedQuantity}.";

                TempData["ToastType"] = "error";

                return RedirectToAction(nameof(Index));
            }

            item.Quantity = quantity;

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Cart updated";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var ownerKey = GetCartOwnerKey();

            var item = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.Id == cartItemId &&
                    (IsLoggedIn
                        ? c.UserId == ownerKey
                        : c.SessionId == ownerKey));

            if (item == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "Item removed from cart";

            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }
    }
}