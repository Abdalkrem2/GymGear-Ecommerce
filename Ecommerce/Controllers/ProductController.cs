using System.Security.Claims;
using Ecommerce.Data;
using Ecommerce.Models.Enums;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        private const int PageSize = 12;

        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Product
        [HttpGet]
        public async Task<IActionResult> Index(
            string? category,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            var query = _context.Products
                .AsNoTracking()
                .Where(product => product.IsActive);

            /*
             * /Product?category=mens
             * /Product?category=womens
             */
            if (!string.IsNullOrWhiteSpace(category))
            {
                if (category.Equals(
                    "mens",
                    StringComparison.OrdinalIgnoreCase))
                {
                    /*
                     * Includes:
                     * Men
                     * Men - Shirts
                     * Men's Activewear
                     */
                    query = query.Where(product =>
                        product.Category != null &&
                        product.Category.Name.StartsWith("Men"));
                }
                else if (category.Equals(
                    "womens",
                    StringComparison.OrdinalIgnoreCase))
                {
                    /*
                     * Includes:
                     * Women
                     * Women - Shirts
                     * Women's Activewear
                     */
                    query = query.Where(product =>
                        product.Category != null &&
                        product.Category.Name.StartsWith("Women"));
                }
            }

            // Filter by specific category
            if (categoryId.HasValue)
            {
                query = query.Where(product =>
                    product.CategoryId == categoryId.Value);
            }

            // Minimum price
            if (minPrice.HasValue)
            {
                query = query.Where(product =>
                    product.Price >= minPrice.Value);
            }

            // Maximum price
            if (maxPrice.HasValue)
            {
                query = query.Where(product =>
                    product.Price <= maxPrice.Value);
            }

            var totalCount = await query.CountAsync();

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalCount / (double)PageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }

            // Sorting
            query = sort switch
            {
                "price_asc" =>
                    query.OrderBy(product => product.Price),

                "price_desc" =>
                    query.OrderByDescending(product => product.Price),

                "newest" =>
                    query.OrderByDescending(product => product.CreatedAt),

                _ =>
                    query.OrderBy(product => product.Name)
            };

            // Products
            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
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

            /*
             * Categories:
             * Shop Men shows Men categories only.
             * Shop Women shows Women categories only.
             * Normal Shop shows all categories.
             */
            var categoriesQuery = _context.Categories
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                if (category.Equals(
                    "mens",
                    StringComparison.OrdinalIgnoreCase))
                {
                    categoriesQuery = categoriesQuery.Where(item =>
                        item.Name.StartsWith("Men"));
                }
                else if (category.Equals(
                    "womens",
                    StringComparison.OrdinalIgnoreCase))
                {
                    categoriesQuery = categoriesQuery.Where(item =>
                        item.Name.StartsWith("Women"));
                }
            }

            var categories = await categoriesQuery
                .OrderBy(item => item.Name)
                .Select(item => new CategoryNavVM
                {
                    Id = item.Id,
                    Name = item.Name
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

            // Preserve mens/womens while filtering
            ViewBag.SelectedCollection = category;

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
                .FirstOrDefaultAsync(product => product.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var reviews = await _context.Comments
                .AsNoTracking()
                .Where(comment =>
                    comment.ProductId == id &&
                    comment.IsApproved)
                .OrderByDescending(comment => comment.CreatedAt)
                .Select(comment => new CommentVM
                {
                    UserFullName = comment.User != null
                        ? comment.User.FullName
                        : string.Empty,

                    Rating = comment.Rating,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                })
                .ToListAsync();

            var isEligibleToReview = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    isEligibleToReview = await _context.Orders
                        .AsNoTracking()
                        .AnyAsync(o => o.UserId == currentUserId &&
                                       o.Status == OrderStatus.Completed &&
                                       o.OrderItems.Any(oi => oi.ProductId == id));
                }
            }

            var model = new ProductDetailsVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                Stock = product.Stock,
                HasSizes = product.HasSizes,

                CategoryName = product.Category != null
                    ? product.Category.Name
                    : string.Empty,

                Images = product.Images
                    .OrderByDescending(image => image.IsMain)
                    .ThenBy(image => image.Id)
                    .Select(image => new ProductImageVM
                    {
                        Id = image.Id,
                        ImagePath = image.ImagePath,
                        IsMain = image.IsMain
                    })
                    .ToList(),

                AverageRating = reviews.Count > 0
                    ? reviews.Average(review => review.Rating)
                    : 0,

                ReviewCount = reviews.Count,
                Reviews = reviews,

                NewComment = new CommentSubmitVM
                {
                    ProductId = product.Id
                },

                IsEligibleToReview = isEligibleToReview
            };

            return View(model);
        }
    }
}