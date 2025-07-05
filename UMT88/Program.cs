using Microsoft.EntityFrameworkCore;
using UMT88.Data;  // namespace AppDbContext

var builder = WebApplication.CreateBuilder(args);

// 1. Đọc chuỗi kết nối
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Thêm Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// 4. Thêm controller & view
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// 6. Bật Session trước Authorization
app.UseSession();

app.UseAuthorization();

// 7. Map routes
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
