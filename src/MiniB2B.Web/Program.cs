using Microsoft.AspNetCore.Authentication.Cookies;
using MiniB2B.Web.Data;
using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + Areas (Admin panel Areas/Admin altında yaşıyor)
builder.Services.AddControllersWithViews();

// Data katmanı
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductGridColumnRepository, ProductGridColumnRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IBannerRepository, BannerRepository>();

// Servisler
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductGridColumnService, ProductGridColumnService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IProductImageUploadService, ProductImageUploadService>();

// Cookie tabanlı authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Veritabanı Users tablosu 01_schema.sql ile önceden kurulmuş olmalıdır.
// Burada sadece varsayılan admin kullanıcısını (yoksa) oluşturuyoruz.
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbInitializer.SeedAdminUserAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Admin kullanıcı seed işlemi başarısız oldu. Veritabanının (01_schema.sql ile) kurulduğundan emin olun.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Area route'u (Admin) önce, sonra varsayılan route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
