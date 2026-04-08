using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.Data;
using SCMS.Interfaces;
using SCMS.Services;

namespace SCMS.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    [Route("admin/[controller]")]
    public class SocialMediaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RazorRenderer _razorRenderer;
        private readonly IThemeEngine _themeEngine;

        public SocialMediaController(ApplicationDbContext context, RazorRenderer razorRenderer, IThemeEngine themeEngine)
        {
            _context = context;
            _razorRenderer = razorRenderer;
            _themeEngine = themeEngine;
        }

        [HttpGet("/admin/socialmedia")]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings
                .Include(s => s.SocialLinks)
                .ThenInclude(link => link.Platform)
                .FirstOrDefaultAsync() ?? new SiteSettings();

            var platforms = await _context.SocialMediaPlatforms.ToListAsync();

            var html = await _razorRenderer.RenderViewAsync(
                HttpContext,
                "/Views/Admin/SocialMedia/Index.cshtml",
                settings,
                new Dictionary<string, object>
                {
                    ["Platforms"] = platforms
                }
            );

            var wrapped = await _themeEngine.RenderAsync(new PageContent
            {
                Title = "Manage Social Media Links",
                HtmlContent = html
            }, _context);

            return Content(wrapped, "text/html");
        }

        [HttpPost("AddLink")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLink([FromForm] int PlatformId, [FromForm] string Url, [FromForm] string? IconColor)
        {
            var settings = await _context.SiteSettings.Include(s => s.SocialLinks).FirstOrDefaultAsync();
            if (settings == null) return NotFound();

            if (settings.SocialLinks.Any(l => l.SocialMediaPlatformId == PlatformId))
            {
                ModelState.AddModelError("", "This platform already has a link.");
                return Redirect("/admin/socialmedia");
            }

            var link = new SiteSocialLink
            {
                SiteSettingsId = settings.Id,
                SocialMediaPlatformId = PlatformId,
                Url = Url,
                IconColor = IconColor
            };

            _context.SiteSocialLinks.Add(link);
            await _context.SaveChangesAsync();
            return Redirect("/admin/socialmedia");
        }

        [HttpPost("DeleteLink")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLink([FromForm] int Id)
        {
            var link = await _context.SiteSocialLinks.FindAsync(Id);
            if (link != null)
            {
                _context.SiteSocialLinks.Remove(link);
                await _context.SaveChangesAsync();
            }

            return Redirect("/admin/socialmedia");
        }
    }
}
