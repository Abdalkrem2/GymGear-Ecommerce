using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using GymGear.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    public class OrderController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderAdminVM
                {
                    Id = o.Id,
                    CustomerName = o.User.FullName,
                    OrderDate = o.OrderDate,
                    ItemCount = o.OrderItems.Count,
                    Total = o.Total,
                    Status = o.Status.ToString()
                })
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var model = new OrderDetailsAdminVM
            {
                Id = order.Id,
                CustomerName = order.User.FullName,
                CustomerEmail = order.User.Email,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                Total = order.Total,
                ShippingAddress = order.ShippingAddress,
                Items = order.OrderItems.Select(oi => new OrderLineItemVM
                {
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.UnitPrice * oi.Quantity
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            if (Enum.TryParse<OrderStatus>(status, out var parsedStatus))
            {
                order.Status = parsedStatus;
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Order status updated";
                TempData["ToastType"] = "success";
            }

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}