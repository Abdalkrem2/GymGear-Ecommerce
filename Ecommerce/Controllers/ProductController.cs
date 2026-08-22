using Ecommerce.Models.ViewModels;
using Ecommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymGear.Web.Data;

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
        // GET: /Product?categoryId=3&minPrice=10&maxPrice=100&sort=price_asc&page=1
        [HttpGet]
        public async Task<IActionResult> Index(
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page = 1)
        {
            if (page < 1)
                page = 1;

            var query = _context.Products
                .AsNoTracking()
                .Where(product => product.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(
                    product =>
                        product.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(
                    product =>
                        product.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(
                    product =>
                        product.Price <= maxPrice.Value);
            }

            var totalCount = await query.CountAsync();

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalCount / (double)PageSize));

            if (page > totalPages)
                page = totalPages;

            query = sort switch
            {
                "price_asc" =>
                    query.OrderBy(product => product.Price),

                "price_desc" =>
                    query.OrderByDescending(
                        product => product.Price),

                "newest" =>
                    query.OrderByDescending(
                        product => product.CreatedAt),

                _ =>
                    query.OrderBy(product => product.Name)
            };

            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(product => new ProductCardVM
                {
                    Id = product.Id,
                    Name = product.Name,

                    CategoryName =
                        product.Category != null
                            ? product.Category.Name
                            : string.Empty,

                    Price = product.Price,

                    MainImagePath = product.Images
                        .OrderByDescending(
                            image => image.IsMain)
                        .ThenBy(image => image.Id)
                        .Select(image => image.ImagePath)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(category => new CategoryNavVM
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToListAsync();

            var model = new ProductListVM
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId,
                CurrentPage = page,
                TotalPages = totalPages,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sort
            };

            return View(model);
        }

        // GET: /Product/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .Include(product => product.Images)
                .Where(product => product.IsActive)
                .FirstOrDefaultAsync(
                    product => product.Id == id);

            if (product == null)
                return NotFound();

            var model = new ProductDetailsVM
            {
                Id = product.Id,
                Name = product.Name,

                Description =
                    product.Description ?? string.Empty,

                Price = product.Price,
                Stock = product.Stock,

                CategoryName =
                    product.Category != null
                        ? product.Category.Name
                        : string.Empty,

                Images = product.Images
                    .OrderByDescending(
                        image => image.IsMain)
                    .ThenBy(image => image.Id)
                    .Select(image => new ProductImageVM
                    {
                        Id = image.Id,
                        ImagePath = image.ImagePath,
                        IsMain = image.IsMain
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}