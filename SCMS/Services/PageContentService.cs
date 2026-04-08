using System.Text.RegularExpressions;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using SCMS.Data;
using SCMS.Models;

namespace SCMS.Services
{
    public class PageContentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly HtmlSanitizer _sanitizer;

        public PageContentService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _sanitizer = new HtmlSanitizer();
            _sanitizer.AllowedTags.Add("iframe");
            _sanitizer.AllowedAttributes.Add("class");
            _sanitizer.AllowedAttributes.Add("style");
            _sanitizer.AllowedAttributes.Add("target");
            _sanitizer.AllowedAttributes.Add("rel");
            _sanitizer.AllowedSchemes.Add("data");
        }

        public async Task<bool> SaveMenuItemContentAsync(MenuItemUpdateModel model)
        {
            var item = await _context.MenuItems
                .Include(m => m.PageContent)
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            if (item == null) return false;

            item.Title = model.Title;
            item.Url = model.IsExternal ? model.Url : null;
            item.IsVisible = model.IsVisible;
            item.SecurityLevelId = model.SecurityLevelId;

            var htmlContent = _sanitizer.Sanitize(model.HtmlContent ?? "");

            if (!model.IsExternal)
            {
                htmlContent = MigrateImagePaths(htmlContent, model.SecurityLevelId);

                if (item.PageContent == null)
                {
                    item.PageContent = new PageContent
                    {
                        Title = model.PageTitle ?? item.Title,
                        HtmlContent = htmlContent,
                        MetaDescription = model.MetaDescription,
                        MetaKeywords = string.Join(", ", model.MetaKeywords ?? new List<string>())
                    };
                }
                else
                {
                    item.PageContent.Title = model.PageTitle ?? item.Title;
                    item.PageContent.HtmlContent = htmlContent;
                    item.PageContent.MetaDescription = model.MetaDescription;
                    item.PageContent.MetaKeywords = string.Join(", ", model.MetaKeywords ?? new List<string>());
                }
            }
            else
            {
                if (item.PageContent != null)
                    _context.PageContents.Remove(item.PageContent);

                item.PageContent = null;
                item.PageContentId = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private string MigrateImagePaths(string htmlContent, int securityLevelId)
        {
            var matches = Regex.Matches(
                htmlContent,
                @"<img[^>]*?src=['""](?<src>(?:\.\./)*(media/secure|uploads/(temp|public|protected))/[^'""]+)['""]",
                RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var rawSrc = match.Groups["src"].Value;
                var currentSrc = rawSrc.TrimStart('.', '/');
                var fileName = Path.GetFileName(currentSrc);

                string oldPath;
                if (currentSrc.StartsWith("media/secure", StringComparison.OrdinalIgnoreCase))
                    oldPath = Path.Combine(_env.ContentRootPath, "uploads", "protected", fileName);
                else if (currentSrc.StartsWith("uploads/public", StringComparison.OrdinalIgnoreCase))
                    oldPath = Path.Combine(_env.WebRootPath, "uploads", "public", fileName);
                else
                    oldPath = Path.Combine(_env.WebRootPath, currentSrc.Replace('/', Path.DirectorySeparatorChar));

                string newPath, newSrc;
                if (securityLevelId == 3)
                {
                    newPath = Path.Combine(_env.WebRootPath, "uploads", "public", fileName);
                    newSrc = $"/uploads/public/{fileName}";
                }
                else
                {
                    newPath = Path.Combine(_env.ContentRootPath, "uploads", "protected", fileName);
                    newSrc = $"/media/secure/{fileName}";
                }

                if (File.Exists(oldPath))
                {
                    var dir = Path.GetDirectoryName(newPath);
                    if (dir != null) Directory.CreateDirectory(dir);
                    File.Move(oldPath, newPath, overwrite: true);
                    htmlContent = htmlContent.Replace(rawSrc, newSrc);
                }
            }

            return htmlContent;
        }
    }
}
