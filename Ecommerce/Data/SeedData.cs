using Ecommerce.Data;
using Ecommerce.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymGear.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // 1) Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2) Default Admin account
            const string adminEmail = "admin@gymgear.com";
            const string adminPassword = "Admin@123"; // change after first login

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null) 
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3) Sample categories (only if none exist yet)
            if (!await context.Categories.AnyAsync())
            {
                context.Categories.AddRange(
                    new Category { Name = "Men's Activewear", Description = "T-shirts, shorts, tracksuits" },
                    new Category { Name = "Women's Activewear", Description = "Leggings, sports bras, tops" },
                    new Category { Name = "Gym Accessories", Description = "Gloves, belts, bottles, bags" },
                    new Category { Name = "Footwear", Description = "Training and running shoes" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
