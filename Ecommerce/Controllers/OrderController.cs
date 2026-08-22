using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == currentUserId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var model = new MyOrderListVM
            {
                Orders = orders.Select(order => new MyOrderVM
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    ItemCount = order.OrderItems.Count,
                    Total = order.Total,
                    Status = order.Status.ToString()
                }).ToList()
            };

            return View(model);
        }

        // GET: /Order/Invoice/5
        public async Task<IActionResult> Invoice(int id)
        {
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.UserId == currentUserId);

            if (order == null)
            {
                return NotFound();
            }

            var model = new InvoiceVM
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                Total = order.Total,
                ShippingAddress = order.ShippingAddress,

                Items = order.OrderItems.Select(oi =>
                    new OrderLineItemVM
                    {
                        ProductName =
                            oi.Product?.Name ?? "Product unavailable",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        LineTotal = oi.UnitPrice * oi.Quantity
                    }).ToList()
            };

            return View(model);
        }
    }
}