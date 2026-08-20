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

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardVM
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync()
            };
            return View(model);
        }
    }
}