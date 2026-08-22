using Ecommerce.Models.ViewModels;
using Ecommerce.Data;
using Ecommerce.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymGear.Web.Data;


namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Admin/Category
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Select(category => new CategoryVM
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description ?? string.Empty,
                    ImagePath = category.ImagePath ?? string.Empty,
                    ProductCount = category.Products.Count
                })
                .ToListAsync();

            return View(categories);
        }

        // GET: /Admin/Category/Create
        public IActionResult Create()
        {
            return View(new CategoryFormVM());
        }

        // POST: /Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? imagePath = null;

            if (model.ImageFile != null)
            {
                imagePath = await SaveImageAsync(model.ImageFile);

                if (!ModelState.IsValid)
                    return View(model);
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                ImagePath = imagePath
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "Category created successfully";

            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Category/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            var model = new CategoryFormVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description ?? string.Empty,
                ExistingImagePath = category.ImagePath ?? string.Empty
            };

            return View(model);
        }

        // POST: /Admin/Category/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CategoryFormVM model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            if (model.ImageFile != null)
            {
                var newImagePath =
                    await SaveImageAsync(model.ImageFile);

                if (!ModelState.IsValid)
                    return View(model);

                DeleteImage(category.ImagePath);
                category.ImagePath = newImagePath;
            }

            category.Name = model.Name;
            category.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "Category updated successfully";

            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Category/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(category => category.Id == id)
                .Select(category => new CategoryVM
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description =
                        category.Description ?? string.Empty,
                    ImagePath =
                        category.ImagePath ?? string.Empty,
                    ProductCount = category.Products.Count
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Admin/Category/Delete/{id}
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(category => category.Products)
                .FirstOrDefaultAsync(
                    category => category.Id == id);

            if (category == null)
                return NotFound();

            if (category.Products.Any())
            {
                TempData["ToastMessage"] =
                    "This category cannot be deleted because it contains products.";

                TempData["ToastType"] = "error";

                return RedirectToAction(nameof(Index));
            }

            DeleteImage(category.ImagePath);

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "Category deleted successfully";

            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveImageAsync(
            IFormFile imageFile)
        {
            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(CategoryFormVM.ImageFile),
                    "Only JPG, JPEG, PNG, and WEBP images are allowed.");

                return null;
            }

            const long maximumSize = 5 * 1024 * 1024;

            if (imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(CategoryFormVM.ImageFile),
                    "Please select a valid image.");

                return null;
            }

            if (imageFile.Length > maximumSize)
            {
                ModelState.AddModelError(
                    nameof(CategoryFormVM.ImageFile),
                    "The image size cannot exceed 5 MB.");

                return null;
            }

            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "categories");

            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(
                fullPath,
                FileMode.Create);

            await imageFile.CopyToAsync(stream);

            return $"/uploads/categories/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            var fileName = Path.GetFileName(imagePath);

            var fullPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "categories",
                fileName);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}