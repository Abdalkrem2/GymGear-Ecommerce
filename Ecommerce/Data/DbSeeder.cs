using Ecommerce.Models.Entities;
using GymGear.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDefaultTestimonialsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure database is created/migrated
            await context.Database.MigrateAsync();

            // Ensure at least one system user exists to attach testimonials to
            var adminUser = await userManager.FindByEmailAsync("admin@gymgear.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gymgear.com",
                    Email = "admin@gymgear.com",
                    FullName = "GymGear Verified Customer",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "P@ssword123!");
            }

            // Only seed if no approved testimonials exist
            if (!await context.Testimonials.AnyAsync(t => t.IsApproved))
            {
                var fixedTestimonials = new List<Testimonial>
                {
                    new Testimonial
                    {
                        UserId = adminUser.Id,
                        Text = "The quality and fit of the seamless collection are unmatched. Easily competes with top-tier athletic brands.",
                        IsApproved = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new Testimonial
                    {
                        UserId = adminUser.Id,
                        Text = "Fast shipping and the material holds up through heavy training sessions without losing shape.",
                        IsApproved = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new Testimonial
                    {
                        UserId = adminUser.Id,
                        Text = "Great customer service and premium apparel. The oversized pump covers are a wardrobe essential.",
                        IsApproved = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    }
                };

                await context.Testimonials.AddRangeAsync(fixedTestimonials);
                await context.SaveChangesAsync();
            }
        }
    }
}
