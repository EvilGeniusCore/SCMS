using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.Data;
using SCMS.Interfaces;
using SCMS.Services;
using SCMS.Services.Theme;

namespace SCMS.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    [Route("admin/[controller]")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RazorRenderer _razorRenderer;
        private readonly IThemeEngine _themeEngine;
        private readonly IThemeManager _themeManager;

        public SettingsController(ApplicationDbContext context, RazorRenderer razorRenderer, IThemeEngine themeEngine, IThemeManager themeManager)
        {
            _context = context;
            _razorRenderer = razorRenderer;
            _themeEngine = themeEngine;
            _themeManager = themeManager;
        }

        [HttpGet("/admin/settings")]
        public async Task<IActionResult> Settings()
        {
            await SyncThemesFromDisk();

            var settings = await _context.SiteSettings.FirstOrDefaultAsync() ?? new SiteSettings();

            var themes = await _context.ThemeSettings.ToListAsync();

            var html = await _razorRenderer.RenderViewAsync(
                HttpContext,
                "/Views/Admin/Settings/Index.cshtml",
                settings,
                new Dictionary<string, object>
                {
                    ["Themes"] = themes.Select(t => new {
                        t.Id,
                        DisplayName = t.DisplayName ?? t.Name,
                        IsSelected = settings.ThemeId == t.Id
                    }).ToList<dynamic>()
                }
            );

            var wrapped = await _themeEngine.RenderAsync(new PageContent
            {
                Title = "Site Settings",
                HtmlContent = html
            }, _context);

            return Content(wrapped, "text/html");
        }

        private async Task SyncThemesFromDisk()
        {
            var themesDir = Path.Combine(Directory.GetCurrentDirectory(), "Themes");
            if (!Directory.Exists(themesDir)) return;

            var existingThemes = await _context.ThemeSettings.ToListAsync();
            var existingNames = existingThemes.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var added = false;

            foreach (var dir in Directory.GetDirectories(themesDir))
            {
                var configPath = Path.Combine(dir, "theme.config.json");
                if (!System.IO.File.Exists(configPath)) continue;

                var folderName = Path.GetFileName(dir);
                if (existingNames.Contains(folderName)) continue;

                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(configPath);
                    var config = JsonSerializer.Deserialize<JsonElement>(json);

                    var theme = new ThemeSetting
                    {
                        Name = config.TryGetProperty("name", out var n) ? n.GetString() ?? folderName : folderName,
                        DisplayName = config.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? folderName : folderName,
                        Description = config.TryGetProperty("description", out var d) ? d.GetString() : null,
                        PreviewImage = config.TryGetProperty("previewImage", out var p) ? p.GetString() : null,
                        SetOn = DateTime.UtcNow
                    };

                    _context.ThemeSettings.Add(theme);
                    added = true;
                }
                catch
                {
                    // Skip themes with malformed config
                }
            }

            if (added)
                await _context.SaveChangesAsync();
        }

        [HttpPost("save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] SiteSettings siteSettings)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid settings: " + string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)));

            var current = await _context.SiteSettings.FirstOrDefaultAsync();
            if (current == null)
            {
                _context.SiteSettings.Add(siteSettings);
            }
            else
            {
                current.SiteName = siteSettings.SiteName;
                current.Tagline = siteSettings.Tagline;
                current.ThemeId = siteSettings.ThemeId;
                current.ContactEmail = siteSettings.ContactEmail;
                current.ContactPhone = siteSettings.ContactPhone;
                current.ContactAddress = siteSettings.ContactAddress;
                current.Copyright = siteSettings.Copyright;
                current.SocialLinks = siteSettings.SocialLinks;
                _context.SiteSettings.Update(current);
            }

            await _context.SaveChangesAsync();
            _themeManager.InvalidateCache();
            return Redirect("/admin/settings");
        }



    }
}
