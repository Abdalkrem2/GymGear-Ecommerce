using Ecommerce.Data;
using Ecommerce.Models.Entities;
using Ecommerce.Models.ViewModels;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Admin/Product
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Select(product => new ProductAdminVM
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryName = product.Category != null
                        ? product.Category.Name
                        : string.Empty,
                    Price = product.Price,
                    Stock = product.Stock,
                    HasSizes = product.HasSizes,
                    IsActive = product.IsActive,
                    MainImagePath = product.Images
                        .OrderByDescending(image => image.IsMain)
                        .ThenBy(image => image.Id)
                        .Select(image => image.ImagePath)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            return View(products);
        }

        // GET: /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            var model = new ProductFormVM();

            await PopulateCategoriesAsync(model);

            return View(model);
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormVM model)
        {
            await ValidateCategoryAsync(model.CategoryId);

            ValidateImages(model.ImageFiles);

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model);
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Stock = model.Stock,
                HasSizes = model.HasSizes,
                CategoryId = model.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (model.ImageFiles.Any())
            {
                await SaveImagesAsync(
                    model.ImageFiles,
                    product.Id,
                    hasMainImage: false);

                await _context.SaveChangesAsync();
            }

            TempData["ToastMessage"] =
                "Product created successfully";

            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(product => product.Images)
                .FirstOrDefaultAsync(product => product.Id == id);

            if (product == null)
                return NotFound();

            var model = new ProductFormVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                Stock = product.Stock,
                HasSizes = product.HasSizes,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                ExistingImages = product.Images
                    .OrderByDescending(image => image.IsMain)
                    .ThenBy(image => image.Id)
                    .Select(image => new ProductImageVM
                    {
                        Id = image.Id,
                        ImagePath = image.ImagePath,
                        IsMain = image.IsMain
                    })
                    .ToList()
            };

            await PopulateCategoriesAsync(model);

            return View(model);
        }

        // POST: /Admin/Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductFormVM model)
        {
            if (id != model.Id)
                return BadRequest();

            var product = await _context.Products
                .Include(product => product.Images)
                .FirstOrDefaultAsync(product => product.Id == id);

            if (product == null)
                return NotFound();

            await ValidateCategoryAsync(model.CategoryId);

            ValidateImages(model.ImageFiles);

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model);

                model.ExistingImages = product.Images
                    .OrderByDescending(image => image.IsMain)
                    .ThenBy(image => image.Id)
                    .Select(image => new ProductImageVM
                    {
                        Id = image.Id,
                        ImagePath = image.ImagePath,
                        IsMain = image.IsMain
                    })
                    .ToList();

                return View(model);
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.HasSizes = model.HasSizes;
            product.IsActive = model.IsActive;
            product.CategoryId = model.CategoryId;

            if (model.ImageFiles.Any())
            {
                var hasMainImage =
                    product.Images.Any(image => image.IsMain);

                await SaveImagesAsync(
                    model.ImageFiles,
                    product.Id,
                    hasMainImage);
            }

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "Product updated successfully";

            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(product => product.Id == id)
                .Select(product => new ProductAdminVM
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryName = product.Category != null
                        ? product.Category.Name
                        : string.Empty,
                    Price = product.Price,
                    Stock = product.Stock,
                    HasSizes = product.HasSizes,
                    IsActive = product.IsActive,
                    MainImagePath = product.Images
                        .OrderByDescending(image => image.IsMain)
                        .ThenBy(image => image.Id)
                        .Select(image => image.ImagePath)
                        .FirstOrDefault() ?? string.Empty
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Admin/Product/Delete/{id}
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(product => product.Id == id);

            if (product == null)
                return NotFound();

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "Product deleted successfully";

            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync(
            ProductFormVM model)
        {
            model.Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(category => new CategoryNavVM
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToListAsync();
        }

        private async Task ValidateCategoryAsync(int categoryId)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(category => category.Id == categoryId);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(ProductFormVM.CategoryId),
                    "Please select a valid category.");
            }
        }

        private void ValidateImages(
            IEnumerable<IFormFile> imageFiles)
        {
            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            const long maximumSize = 5 * 1024 * 1024;

            foreach (var imageFile in imageFiles)
            {
                var extension = Path
                    .GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

                if (imageFile.Length == 0)
                {
                    ModelState.AddModelError(
                        nameof(ProductFormVM.ImageFiles),
                        "Please select valid images.");

                    continue;
                }

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(ProductFormVM.ImageFiles),
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }

                if (imageFile.Length > maximumSize)
                {
                    ModelState.AddModelError(
                        nameof(ProductFormVM.ImageFiles),
                        "Each image must not exceed 5 MB.");
                }
            }
        }

        private async Task SaveImagesAsync(
            IEnumerable<IFormFile> imageFiles,
            int productId,
            bool hasMainImage)
        {
            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "products");

            Directory.CreateDirectory(folderPath);

            foreach (var imageFile in imageFiles)
            {
                var extension = Path
                    .GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

                var fileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(folderPath, fileName);

                await using var stream = new FileStream(
                    fullPath,
                    FileMode.Create);

                await imageFile.CopyToAsync(stream);

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImagePath =
                        $"/uploads/products/{fileName}",
                    IsMain = !hasMainImage
                };

                _context.ProductImages.Add(productImage);

                hasMainImage = true;
            }
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            var fileName = Path.GetFileName(imagePath);

            var fullPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "products",
                fileName);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}