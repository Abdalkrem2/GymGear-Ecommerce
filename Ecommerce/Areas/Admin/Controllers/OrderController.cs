using Ecommerce.Data;
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
    }
}