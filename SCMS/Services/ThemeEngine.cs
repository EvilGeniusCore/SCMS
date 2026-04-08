using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SCMS.Data;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SCMS.Classes;
using Microsoft.AspNetCore.Antiforgery;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using SCMS.Interfaces;
using SCMS.Services.Theme;

namespace SCMS.Services
{
    public class ThemeEngine : IThemeEngine
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IThemeManager _themeManager;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ThemeEngine> _logger;

        public ThemeEngine(IHttpContextAccessor httpContextAccessor, IThemeManager themeManager, IMemoryCache cache, ILogger<ThemeEngine> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _themeManager = themeManager;
            _cache = cache;
            _logger = logger;
        }

        private async Task<string> ReadThemeFileAsync(string path)
        {
            var cacheKey = $"ThemeFile:{path}";
            if (_cache.TryGetValue(cacheKey, out string? cached) && cached != null)
                return cached;

            var content = await File.ReadAllTextAsync(path);
            _cache.Set(cacheKey, content, TimeSpan.FromMinutes(30));
            return content;
        }

        public async Task<string> RenderAsync(PageContent page, ApplicationDbContext db)
        {
            var siteSettings = await db.SiteSettings
                .Include(s => s.SocialLinks)
                .ThenInclude(l => l.Platform)
                .FirstOrDefaultAsync();

            var themeName = await _themeManager.GetCurrentThemeAsync(db);
            var themePath = Path.Combine("Themes", themeName);

            string layout, template, header, footer;
            try
            {
                layout = await ReadThemeFileAsync(Path.Combine(themePath, "layout.html"));
                template = await ReadThemeFileAsync(Path.Combine(themePath, "templates", "page.html"));
                header = await ReadThemeFileAsync(Path.Combine(themePath, "partials", "header.html"));
                footer = await ReadThemeFileAsync(Path.Combine(themePath, "partials", "footer.html"));
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Theme file missing for theme '{ThemeName}' at path '{ThemePath}'", themeName, themePath);
                return $"<html><body><h1>Theme Error</h1><p>A required theme file is missing: {ex.FileName}</p><hr/>{page.HtmlContent}</body></html>";
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Theme directory missing for theme '{ThemeName}' at path '{ThemePath}'", themeName, themePath);
                return $"<html><body><h1>Theme Error</h1><p>Theme directory not found: {themePath}</p><hr/>{page.HtmlContent}</body></html>";
            }

            var faviconPath = Path.Combine("/Themes", themeName, siteSettings?.Theme?.Favicon ?? "favicon.ico");
            var logoUrl = siteSettings?.Logo ?? "/Themes/default/images/SCMS_Logo.png";

            var copyright =
                string.IsNullOrWhiteSpace(siteSettings?.Copyright)
                ? $"\u00a9 {DateTime.Now.Year} {siteSettings?.SiteName ?? "SCMS"}"
                : siteSettings.Copyright;

            var tagline = siteSettings?.Tagline ?? "Site Powered by SCMS";

            var bodyContent = page.HtmlContent ?? "";

            // Inject TempData["Error"] if it exists
            var httpContext = _httpContextAccessor.HttpContext;
            var tempData = httpContext?.RequestServices
                .GetService<ITempDataDictionaryFactory>()
                ?.GetTempData(httpContext);

            var body = template
                .Replace("<cms:PageTitle />", page.Title ?? "")
                .Replace("<cms:Content />", bodyContent);

            var result = layout
                .Replace("<cms:Header />", header)
                .Replace("<cms:Footer />", footer)
                .Replace("<cms:PageTitle />", page.Title ?? "")
                .Replace("<cms:Content />", body)
                .Replace("<cms:Favicon />", $"<link rel=\"icon\" href=\"{faviconPath}\" type=\"image/x-icon\">")
                .Replace("<cms:Copyright />", copyright)
                .Replace("<cms:Tagline />", tagline);

            var user = httpContext?.User;
            bool isAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            string loginStatusHtml = isAuthenticated
                ? "<a href=\"/portal-logout\">Logout</a>"
                : "<a href=\"/portal-access\">Login</a>";
            result = result.Replace("<cms:LoginStatus />", loginStatusHtml);

            // Replace <cms:ErrorMessage /> with alert if TempData["Error"] exists
            if (result.Contains("<cms:ErrorMessage />"))
            {
                var err = tempData?["Error"] as string;
                var errorHtml = !string.IsNullOrEmpty(err)
                    ? $"<div class='alert alert-danger'>{err}</div>"
                    : "";
                result = result.Replace("<cms:ErrorMessage />", errorHtml);
            }

            // site name
            result = Regex.Replace(result, @"<cms:SiteName\s*\/>", match =>
            {
                return siteSettings?.SiteName ?? "Site";
            });

            // Handle <cms:Menu />
            var menuRegex = new Regex(
                @"<cms:Menu\s+(?=.*orientation=""(?<orientation>\w+)""\s*)(?=.*group=""(?<group>[^""]+)""\s*).*?\/?>",
                RegexOptions.IgnoreCase
            );

            result = menuRegex.Replace(result, match =>
            {
                var orientation = match.Groups["orientation"].Value;
                var group = match.Groups["group"].Value;
                var principal = httpContext?.User ?? new ClaimsPrincipal();
                return MenuBuilder.GenerateMenuHtml(db, group, orientation, principal, themeName);
            });

            // Handle <cms:SiteLogo height="..." />
            var logoRegex = new Regex(@"<cms:SiteLogo(?:\s+height\s*=\s*""(?<height>\d+)"")?\s*\/>");
            result = logoRegex.Replace(result, match =>
            {
                var height = match.Groups["height"].Success ? match.Groups["height"].Value : "50";
                return $"<img src=\"{logoUrl}\" alt=\"Site Logo\" style=\"max-height: {height}px;\">";
            });

            var displayName = user?.Identity?.IsAuthenticated == true
                ? (httpContext?.User.Identity?.Name ?? "User")
                : "Guest";
            result = result.Replace("<cms:UserName />", displayName);

            // Handle {{ANTIFORGERY_TOKEN}} replacement
            if (result.Contains("{{ANTIFORGERY_TOKEN}}"))
            {
                var antiforgery = httpContext?.RequestServices.GetService<IAntiforgery>();
                var tokenValue = "";
                if (antiforgery != null && httpContext != null)
                {
                    var tokenSet = antiforgery.GetAndStoreTokens(httpContext);
                    tokenValue = tokenSet.RequestToken ?? "";
                }
                result = result.Replace("{{ANTIFORGERY_TOKEN}}", tokenValue);
            }

            // Handle <cms:SocialLinks />
            var socialLinksRegex = new Regex(@"<cms:SocialLinks\s*\/>", RegexOptions.IgnoreCase);

            if (socialLinksRegex.IsMatch(result))
            {
                var socialTemplatePath = Path.Combine(themePath, "partials", "social.template.html");
                string renderedSocial = "";

                if (File.Exists(socialTemplatePath))
                {
                    var templateText = await ReadThemeFileAsync(socialTemplatePath);

                    var links = siteSettings?.SocialLinks?.Where(l => l.Platform != null).Select(link =>
                        new Dictionary<string, object>
                        {
                            ["Url"] = link.Url,
                            ["Name"] = link.Platform.Name,
                            ["IconClass"] = link.Platform.IconClass,
                            ["IconColor"] = link.IconColor ?? "#000000"
                        }
                    ).ToList<object>() ?? new List<object>();

                    var socialData = new Dictionary<string, object>
                    {
                        ["Items"] = links
                    };

                    var parser = new SCMS.Services.Template.TemplateParser();
                    renderedSocial = parser.Parse(templateText, socialData);
                }

                result = socialLinksRegex.Replace(result, renderedSocial);
            }

            // Handle <cms:Breadcrumb />
            result = Regex.Replace(result, @"<cms:Breadcrumb\s*\/>", match =>
            {
                var requestPath = httpContext?.Request.Path.Value?.Trim('/') ?? "";
                var principal = httpContext?.User ?? new ClaimsPrincipal();

                string html = MenuBuilder.GenerateBreadcrumbHtml(db, requestPath, principal);

                return string.IsNullOrWhiteSpace(html)
                    ? "<nav aria-label=\"breadcrumb\"><ol class=\"breadcrumb mb-0\"><li class=\"breadcrumb-item active\" aria-current=\"page\">Home</li></ol></nav>"
                    : html;
            });

            // Inject Highlight.js CSS and optional spacing patch
            if (!result.Contains("atom-one-dark.min.css"))
            {
                result = result.Replace("</head>", @"
                <link href=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css"" rel=""stylesheet"">
                <link href=""/css/highlight-patch.css"" rel=""stylesheet"">
                </head>");
            }

            // Inject Highlight.js JS and activation script
            if (!result.Contains("highlight.min.js"))
            {
                result = result.Replace("</body>", @"
                <script src=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js""></script>
                <script>hljs.highlightAll();</script>
                </body>");
            }

            // Catch unknown tokens and replace with UNKNOWN — leave at bottom
            result = Regex.Replace(result, @"<cms:[^>]+\/>", match =>
            {
                var safeToken = match.Value.Replace("<", "(").Replace(">", ")");
                return $"<span style='color: red; font-weight: bold;'>[UNKNOWN TOKEN: {safeToken}]</span>";
            });

            return result;
        }
    }
}
