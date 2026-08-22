using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var vm = await BuildCheckoutVMAsync();

            if (vm.Items.Count == 0)
            {
                TempData["ToastMessage"] = "Your cart is empty.";
                TempData["ToastType"] = "info";
                return RedirectToAction("Index", "Cart");
            }

            return View(vm);
        }

        // POST: /Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutVM model)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Re-read the logged-in user's cart items from the DB — don't trust the posted form for pricing/qty.
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                TempData["ToastMessage"] = "Your cart is empty.";
                TempData["ToastType"] = "info";
                return RedirectToAction("Index", "Cart");
            }

            // Validate stock is sufficient before creating anything.
            foreach (var item in cartItems)
            {
                if (item.Product == null || item.Product.Stock < item.Quantity)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Not enough stock for {(item.Product != null ? item.Product.Name : "one of your items")}. " +
                        $"Please update your cart and try again.");
                }
            }

            if (!ModelState.IsValid)
            {
                var vm = await BuildCheckoutVMAsync();
                // Preserve the shipping details the user already typed in.
                vm.FirstName = model.FirstName;
                vm.LastName = model.LastName;
                vm.Email = model.Email;
                vm.StreetAddress = model.StreetAddress;
                vm.City = model.City;
                vm.State = model.State;
                vm.ZipCode = model.ZipCode;
                vm.PhoneNumber = model.PhoneNumber;
                return View(vm);
            }

            var subtotal = cartItems.Sum(c => c.Product!.Price * c.Quantity);

            // 2. Create the Order.
            var order = new Order
            {
                UserId = userId!,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Processing,
                Total = subtotal,
                ShippingAddress =
                    $"{model.FirstName} {model.LastName}, {model.StreetAddress}, {model.City}, {model.State} {model.ZipCode}"
            };

            // 3. One OrderItem per cart item, snapshotting the current price.
            foreach (var item in cartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product!.Price
                });

                // 4. Decrement stock (already validated sufficient above).
                item.Product.Stock -= item.Quantity;
            }

            _context.Orders.Add(order);

            // 5. Clear the cart.
            _context.CartItems.RemoveRange(cartItems);

            // 6. Save everything atomically, then redirect to confirmation.
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Order placed successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Confirmation), new { orderId = order.Id });
        }

        // GET: /Checkout/Confirmation/{orderId}
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderConfirmationVM
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Total = order.Total
            };

            return View(vm);
        }

        private async Task<CheckoutVM> BuildCheckoutVMAsync()
        {
            var userId = _userManager.GetUserId(User);

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Category)
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Images)
                .ToListAsync();

            var items = cartItems.Select(c => new CartItemVM
            {
                CartItemId = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product != null ? c.Product.Name : string.Empty,
                CategoryName = c.Product != null && c.Product.Category != null ? c.Product.Category.Name : string.Empty,
                MainImagePath = c.Product != null
                    ? (c.Product.Images.Where(i => i.IsMain).Select(i => i.ImagePath).FirstOrDefault()
                        ?? c.Product.Images.Select(i => i.ImagePath).FirstOrDefault())
                    : null,
                UnitPrice = c.Product != null ? c.Product.Price : 0,
                Quantity = c.Quantity,
                LineTotal = (c.Product != null ? c.Product.Price : 0) * c.Quantity
            }).ToList();

            return new CheckoutVM
            {
                Items = items,
                Subtotal = items.Sum(i => i.LineTotal)
            };
        }
    }
}