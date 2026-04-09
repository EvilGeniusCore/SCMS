using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCMS.Data;
using SCMS.Interfaces;
using SCMS.Services;
using SCMS.Classes;
using SCMS.Areas.Identity.Services;
using SCMS.Abstractions;
using SCMS.Services.Auth;
using SCMS.Services.Theme;
using SCMS.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Explicit config override setup for environment support
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "..", "secrets", "appsettings.secrets.json"), optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Configure database context with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Add ASP.NET Identity with default settings
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/portal-access";
    options.LogoutPath = "/portal-logout";
    options.AccessDeniedPath = "/access-denied"; // optional, if you have one
});

// Custom signin manager
builder.Services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();

// Add Razor Pages and MVC
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IThemeEngine, ThemeEngine>();
builder.Services.AddSingleton<IThemeManager, SCMS.Services.Theme.ThemeManager>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<PageContentService>();
builder.Services.AddHttpContextAccessor();

// Register built-in token handlers
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.PageTitleTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.ContentTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.FaviconTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.MetaTagsTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.CopyrightTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.TaglineTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.LoginStatusTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.UserNameTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.ErrorMessageTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.SiteNameTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.SiteLogoTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.MenuTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.AntiforgeryTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.SocialLinksTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.BreadcrumbTokenHandler>();
builder.Services.AddSingleton<ITokenHandler, SCMS.Services.TokenHandlers.ModuleAdminMenuTokenHandler>();
// Add the user context service
builder.Services.AddScoped<CurrentUserContext>();
// register the RazorRender for admin pages to use client themes.
builder.Services.AddScoped<RazorRenderer>();

// Discover and load external modules from /Modules folder
using var startupLoggerFactory = LoggerFactory.Create(b => b.AddConsole());
var startupLogger = startupLoggerFactory.CreateLogger("Startup");
ModuleLoader.DiscoverAndRegister(builder.Services, startupLogger);

// build the app
var app = builder.Build();

// Ensure database folder exists BEFORE context resolution
if (!Directory.Exists("Database"))
{
    Directory.CreateDirectory("Database");
}

// Apply migrations and seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
    await IdentitySeeder.SeedAdminUserAsync(services);

    // Run module database setup after core migrations
    await ModuleLoader.RunModuleDbSetupAsync(services, startupLogger);
}

// Configure middleware
app.UseMiddleware<CurrentUserMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Custom CMS route

app.MapControllerRoute(
    name: "default",
    pattern: "{slug?}",
    defaults: new { controller = "Page", action = "RenderPage" });



// Ensure theme assets are in place
ThemeAssetManager.EnsureThemeAssets();

app.Run();

// Expose for integration tests
public partial class Program { }
