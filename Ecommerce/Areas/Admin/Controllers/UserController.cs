using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    public class UserController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Select(u => new UserListItemVM
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    JoinDate = null,   // ApplicationUser has no CreatedAt field yet — leave null for now
                    OrderCount = 0     // Orders don't exist until Phase 4
                })
                .ToListAsync();

            return View(users);
        }
    }
}