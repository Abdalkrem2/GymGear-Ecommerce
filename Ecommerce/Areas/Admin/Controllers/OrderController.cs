using Ecommerce.Data;
using Ecommerce.Models.Enums;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
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

        // GET: /Admin/Order
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            string? sort)
        {
            var query = _context.Orders
                .AsNoTracking()
                .AsQueryable();

            // Search by order ID or customer name.
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                var orderNumberText = search
                    .Replace("#", "")
                    .Replace(
                        "ORD-",
                        "",
                        StringComparison.OrdinalIgnoreCase);

                if (int.TryParse(
                    orderNumberText,
                    out var orderId))
                {
                    query = query.Where(o =>
                        o.Id == orderId ||
                        (o.User != null &&
                         o.User.FullName.Contains(search)));
                }
                else
                {
                    query = query.Where(o =>
                        o.User != null &&
                        o.User.FullName.Contains(search));
                }
            }

            // Filter by order status.
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<OrderStatus>(
                    status,
                    true,
                    out var parsedStatus))
            {
                query = query.Where(o =>
                    o.Status == parsedStatus);
            }

            // Start date is inclusive.
            if (fromDate.HasValue)
            {
                var startDate = fromDate.Value.Date;

                query = query.Where(o =>
                    o.OrderDate >= startDate);
            }

            // End date includes the full selected day.
            if (toDate.HasValue)
            {
                var endDate =
                    toDate.Value.Date.AddDays(1);

                query = query.Where(o =>
                    o.OrderDate < endDate);
            }

            query = sort switch
            {
                "oldest" =>
                    query.OrderBy(o => o.OrderDate),

                "amount_asc" =>
                    query.OrderBy(o => o.Total),

                "amount_desc" =>
                    query.OrderByDescending(o => o.Total),

                _ =>
                    query.OrderByDescending(o => o.OrderDate)
            };

            var orders = await query
                .Select(o => new OrderAdminVM
                {
                    Id = o.Id,

                    CustomerName = o.User != null
                        ? o.User.FullName
                        : "Unknown customer",

                    OrderDate = o.OrderDate,
                    ItemCount = o.OrderItems.Count,
                    Total = o.Total,
                    Status = o.Status.ToString()
                })
                .ToListAsync();

            return View(orders);
        }

        // GET: /Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var model = new OrderDetailsAdminVM
            {
                Id = order.Id,

                CustomerName = order.User?.FullName
                    ?? "Unknown customer",

                CustomerEmail = order.User?.Email
                    ?? string.Empty,

                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                Total = order.Total,
                ShippingAddress = order.ShippingAddress,

                Items = order.OrderItems.Select(oi =>
                    new OrderLineItemVM
                    {
                        ProductName = oi.Product?.Name
                            ?? "Product unavailable",

                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Size = oi.Size,

                        LineTotal =
                            oi.UnitPrice * oi.Quantity
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: /Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int orderId,
            string status)
        {
            var order =
                await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse<OrderStatus>(
                status,
                true,
                out var parsedStatus))
            {
                TempData["ToastMessage"] =
                    "Invalid order status.";

                TempData["ToastType"] = "error";

                return RedirectToAction(
                    nameof(Details),
                    new { id = orderId });
            }

            order.Status = parsedStatus;

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "Order status updated";

            TempData["ToastType"] = "success";

            return RedirectToAction(
                nameof(Details),
                new { id = orderId });
        }
    }
}