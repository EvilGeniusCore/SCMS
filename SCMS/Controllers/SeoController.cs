using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.Data;

namespace SCMS.Controllers
{
    public class SeoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SeoController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("/sitemap.xml")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> Sitemap()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";

            var pages = await _db.PageContents
                .Where(p => p.Visibility == "Public")
                .Select(p => new { p.PageKey, p.LastUpdated })
                .ToListAsync();

            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var urlElements = pages.Select(p =>
            {
                var loc = p.PageKey == "home" ? baseUrl + "/" : $"{baseUrl}/{p.PageKey}";
                var el = new XElement(ns + "url",
                    new XElement(ns + "loc", loc),
                    new XElement(ns + "changefreq", "weekly"),
                    new XElement(ns + "priority", p.PageKey == "home" ? "1.0" : "0.5")
                );

                if (p.LastUpdated != default)
                    el.Add(new XElement(ns + "lastmod", p.LastUpdated.ToString("yyyy-MM-dd")));

                return el;
            });

            var sitemap = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "urlset", urlElements)
            );

            return Content(sitemap.Declaration + "\n" + sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        [HttpGet("/robots.txt")]
        [ResponseCache(Duration = 86400)]
        public IActionResult Robots()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";

            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine();
            sb.AppendLine("# Admin area");
            sb.AppendLine("Disallow: /admin/");
            sb.AppendLine("Disallow: /Identity/");
            sb.AppendLine("Disallow: /portal-access");
            sb.AppendLine("Disallow: /portal-logout");
            sb.AppendLine("Disallow: /media/secure/");
            sb.AppendLine();
            sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");

            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}
