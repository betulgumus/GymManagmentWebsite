using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using webproje1.Data;
using webproje1.Models;
using webproje1.Options;
using webproje1.Services;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// DATABASE
// ===========================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ===========================
// IDENTITY
// ===========================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===========================
// COOKIE AYARLARI
// ===========================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// ===========================
// MVC
// ===========================
builder.Services.AddControllersWithViews();

// ===========================
// ?? GROQ AI SERV?S?
// ===========================

// HttpClient + Service
builder.Services.AddHttpClient<GroqChatService>();

// Options pattern (appsettings.json)
builder.Services.Configure<GroqOptions>(
    builder.Configuration.GetSection("Groq"));

var app = builder.Build();

// ===========================
// ROLE SEED (Admin / Member / Trainer)
// ===========================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Member", "Trainer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// ===========================
// PIPELINE
// ===========================
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

// ===========================
// ROUTING
// ===========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
