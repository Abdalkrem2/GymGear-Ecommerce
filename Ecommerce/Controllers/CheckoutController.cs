using Ecommerce.Data;
using Ecommerce.Models.Entities;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var items = await _context.CartItems
                .Include(c => c.Product).ThenInclude(p => p.Images)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var vm = new CheckoutVM
            {
                Items = items.Select(c => new CartItemVM
                {
                    CartItemId = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    MainImagePath = c.Product.Images.FirstOrDefault()?.ImagePath,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity,
                    Size = c.Size,
                    LineTotal = c.Product.Price * c.Quantity
                }).ToList()
            };
            vm.Subtotal = vm.Items.Sum(i => i.LineTotal);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutVM model)
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ToastMessage"] = "Your cart is empty.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.Items = cartItems.Select(c => new CartItemVM
                {
                    CartItemId = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity,
                    Size = c.Size,
                    LineTotal = c.Product.Price * c.Quantity
                }).ToList();
                model.Subtotal = model.Items.Sum(i => i.LineTotal);
                return View(model);
            }

            // Check stock before creating the order
            foreach (var item in cartItems)
            {
                if (item.Product.Stock < item.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"'{item.Product.Name}' only has {item.Product.Stock} left in stock.");
                    model.Items = cartItems.Select(c => new CartItemVM
                    {
                        CartItemId = c.Id,
                        ProductId = c.ProductId,
                        ProductName = c.Product.Name,
                        UnitPrice = c.Product.Price,
                        Quantity = c.Quantity,
                        Size = c.Size,
                        LineTotal = c.Product.Price * c.Quantity
                    }).ToList();
                    model.Subtotal = model.Items.Sum(i => i.LineTotal);
                    return View(model);
                }
            }

            var total = cartItems.Sum(c => c.Product.Price * c.Quantity);

            var order = new Order
            {
                UserId = userId!,
                OrderDate = DateTime.UtcNow,
                Status = Ecommerce.Models.Enums.OrderStatus.Processing,
                Total = total,
                ShippingAddress = $"{model.FirstName} {model.LastName}, {model.StreetAddress}, {model.City}, {model.State} {model.ZipCode}"
            };
            _context.Orders.Add(order);

            foreach (var item in cartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    Size = item.Size
                });
                item.Product.Stock -= item.Quantity;
            }

            var payment = new Payment
            {
                Order = order,
                Provider = "Stripe (Test)",
                Status = "Pending",
                Amount = total
            };
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            // Create the Stripe Checkout Session
            var domain = $"{Request.Scheme}://{Request.Host}";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(total * 100), // Stripe uses cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Gym Gear & Activewear — Order #{order.Id:D4}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/Checkout/PaymentSuccess?orderId={order.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Checkout/PaymentCancelled?orderId={order.Id}"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }

        public async Task<IActionResult> PaymentSuccess(int orderId, string session_id)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            if (session.PaymentStatus == "paid" && order.Payment != null)
            {
                order.Payment.Status = "Paid";
                order.Payment.TransactionId = session.PaymentIntentId;
                order.Payment.PaidAt = DateTime.UtcNow;

                // Clear the cart only after payment is confirmed
                var userId = _userManager.GetUserId(User);
                var cartItems = _context.CartItems.Where(c => c.UserId == userId);
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Confirmation", new { orderId = order.Id });
        }

        public async Task<IActionResult> PaymentCancelled(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                // Restore stock and remove the unpaid order
                foreach (var item in order.OrderItems)
                {
                    item.Product.Stock += item.Quantity;
                }
                if (order.Payment != null) _context.Payments.Remove(order.Payment);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            TempData["ToastMessage"] = "Payment was cancelled. Your cart has been kept.";
            TempData["ToastType"] = "info";
            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            var vm = new OrderConfirmationVM
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Total = order.Total
            };
            return View(vm);
        }
    }
}