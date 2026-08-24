using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Include today and the previous 6 days.
            var startDate =
                DateTime.UtcNow.Date.AddDays(-6);

            var orderDates = await _context.Orders
                .AsNoTracking()
                .Where(order =>
                    order.OrderDate >= startDate)
                .Select(order => order.OrderDate)
                .ToListAsync();

            var chartLabels = new List<string>();
            var chartValues = new List<int>();

            for (var dayNumber = 0;
                 dayNumber < 7;
                 dayNumber++)
            {
                var day = startDate.AddDays(dayNumber);

                chartLabels.Add(
                    day.ToString("MMM dd"));

                chartValues.Add(
                    orderDates.Count(orderDate =>
                        orderDate.Date == day));
            }

            var recentOrders = await _context.Orders
                .AsNoTracking()
                .OrderByDescending(order =>
                    order.OrderDate)
                .Take(5)
                .Select(order => new OrderAdminVM
                {
                    Id = order.Id,

                    CustomerName = order.User != null
                        ? order.User.FullName
                        : "Unknown customer",

                    OrderDate = order.OrderDate,

                    ItemCount =
                        order.OrderItems.Count,

                    Total = order.Total,
                    Status = order.Status.ToString()
                })
                .ToListAsync();

            var model = new DashboardVM
            {
                TotalUsers =
                    await _context.Users.CountAsync(),

                TotalProducts =
                    await _context.Products.CountAsync(),

                TotalCategories =
                    await _context.Categories.CountAsync(),

                TotalOrders =
                    await _context.Orders.CountAsync(),

                OrderChartLabels = chartLabels,
                OrderChartValues = chartValues,
                RecentOrders = recentOrders
            };

            return View(model);
        }
    }
}