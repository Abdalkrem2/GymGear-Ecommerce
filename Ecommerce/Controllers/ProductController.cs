using Ecommerce.Data;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        private const int PageSize = 9;

        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Product
        // GET: /Product?categoryId=3&page=2
        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages < 1)
            {
                totalPages = 1;
            }

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new ProductCardVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    Price = p.Price,
                    MainImagePath = p.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.ImagePath)
                        .FirstOrDefault()
                        ?? p.Images.Select(i => i.ImagePath).FirstOrDefault()
                })
                .ToListAsync();

            var vm = new ProductListVM
            {
                Products = products,
                Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryNavVM { Id = c.Id, Name = c.Name })
                    .ToListAsync(),
                SelectedCategoryId = categoryId,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        // GET: /Product/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var vm = new ProductDetailsVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = product.Category != null ? product.Category.Name : string.Empty,
                Images = product.Images
                    .OrderByDescending(i => i.IsMain)
                    .Select(i => new ProductImageVM
                    {
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        IsMain = i.IsMain
                    })
                    .ToList()
            };

            return View(vm);
        }
    }
}