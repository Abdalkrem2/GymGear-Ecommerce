using GymGear.Web.Data;
using Ecommerce.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Data;

var builder = WebApplication.CreateBuilder(args);
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// 1) EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

// 2) ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3) MVC + Areas
builder.Services.AddControllersWithViews();

// 4) Session (used for guest cart before login)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 5) Register your services here as each person builds them, e.g.:
// builder.Services.AddScoped<IProductService, ProductService>();
// builder.Services.AddScoped<ICartService, CartService>();
// builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Seed roles/admin/categories on startup
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseStatusCodePagesWithReExecute("/Home/HandleError/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Admin area route (must be registered before the default route)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapRazorPages(); // needed if you use Identity's scaffolded Razor Pages for Login/Register
await DbSeeder.SeedDefaultTestimonialsAsync(app.Services);

app.Run();
