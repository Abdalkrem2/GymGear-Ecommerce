using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Search?q=...
        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var query = q?.Trim() ?? string.Empty;

            var model = new SearchResultsVM
            {
                Query = query
            };

            if (string.IsNullOrWhiteSpace(query))
                return View(model);

            model.Results = await _context.Products
                .AsNoTracking()
                .Where(product =>
                    product.IsActive &&
                    (
                        product.Name.Contains(query) ||
                        (
                            product.Category != null &&
                            product.Category.Name.Contains(query)
                        )
                    ))
                .Select(product => new ProductCardVM
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryName = product.Category != null
                        ? product.Category.Name
                        : string.Empty,
                    Price = product.Price,
                    MainImagePath = product.Images
                        .OrderByDescending(image => image.IsMain)
                        .ThenBy(image => image.Id)
                        .Select(image => image.ImagePath)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            return View(model);
        }
    }
}