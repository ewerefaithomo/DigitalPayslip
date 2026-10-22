using DigitalPayslip.Web.Data;
using DigitalPayslip.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------------
// 1. Configure Database
// --------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------
// 2. Register Services
// --------------------------
builder.Services.AddScoped<IPayslipProcessor, PayslipProcessor>();
builder.Services.AddScoped<IEmailService, EmailService>();

// --------------------------
// 3. Add Controllers with Views and TempData using Session
// --------------------------
builder.Services.AddControllersWithViews()
       .AddSessionStateTempDataProvider(); // <-- TempData uses Session

// --------------------------
// 4. Configure Session
// --------------------------
builder.Services.AddDistributedMemoryCache(); // In-memory cache for session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".DigitalPayslip.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30); // adjust timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// --------------------------
// 5. Middleware Pipeline
// --------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // <-- Must be before UseAuthorization

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
