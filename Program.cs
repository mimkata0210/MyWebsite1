using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWebsite1.Data;
using MyWebsite1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Use EF Core to manage identity data

builder.Services.AddControllersWithViews();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

var app = builder.Build();

// Warm-up EF Core model at startup (fixes first-load slowness)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Photos.Take(1).ToList(); // dummy query to compile model
}

// Configure the HTTP request pipeline.
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

// Explicit Home route for /
app.MapControllerRoute(
    name: "home",
    pattern: "",
    defaults: new { controller = "Home", action = "Index" });

// Clean URLs for Home actions
app.MapControllerRoute(
    name: "about",
    pattern: "About",
    defaults: new { controller = "Home", action = "About" });

app.MapControllerRoute(
    name: "contacts",
    pattern: "Contacts",
    defaults: new { controller = "Home", action = "Contacts" });

app.MapControllerRoute(
    name: "privacy",
    pattern: "Privacy",
    defaults: new { controller = "Home", action = "Privacy" });

// Other controllers
app.MapControllerRoute(
    name: "downloads",
    pattern: "Downloads",
    defaults: new { controller = "Downloads", action = "Index" });

// Default fallback
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity pages like Login, Register, etc.

app.Run();
