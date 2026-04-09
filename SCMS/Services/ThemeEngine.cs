using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SCMS.Data;
using SCMS.Abstractions;
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
        private readonly IEnumerable<ITokenHandler> _tokenHandlers;

        public ThemeEngine(
            IHttpContextAccessor httpContextAccessor,
            IThemeManager themeManager,
            IMemoryCache cache,
            ILogger<ThemeEngine> logger,
            IEnumerable<ITokenHandler> tokenHandlers)
        {
            _httpContextAccessor = httpContextAccessor;
            _themeManager = themeManager;
            _cache = cache;
            _logger = logger;
            _tokenHandlers = tokenHandlers;
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
            // Load site settings
            var siteSettings = await db.SiteSettings
                .Include(s => s.SocialLinks)
                .ThenInclude(l => l.Platform)
                .FirstOrDefaultAsync();

            // Resolve theme and load layout files
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

            // Phase 1: Assemble layout structure (partials into layout, content into template)
            var body = template
                .Replace("<cms:PageTitle />", page.Title ?? "")
                .Replace("<cms:Content />", page.HtmlContent ?? "");

            var result = layout
                .Replace("<cms:Header />", header)
                .Replace("<cms:Footer />", footer)
                .Replace("<cms:Content />", body);

            // Phase 2: Build render context for all token handlers
            var httpContext = _httpContextAccessor.HttpContext;
            var socialLinks = siteSettings?.SocialLinks?
                .Where(l => l.Platform != null)
                .Select(link => new Dictionary<string, object>
                {
                    ["Url"] = link.Url,
                    ["Name"] = link.Platform.Name,
                    ["IconClass"] = link.Platform.IconClass,
                    ["IconColor"] = link.IconColor ?? "#000000"
                })
                .ToList() ?? new List<Dictionary<string, object>>();

            var renderContext = new TokenRenderContext
            {
                PageTitle = page.Title ?? "",
                PageContent = page.HtmlContent ?? "",
                MetaDescription = page.MetaDescription,
                MetaKeywords = page.MetaKeywords,
                SiteName = siteSettings?.SiteName ?? "Site",
                Tagline = siteSettings?.Tagline,
                Copyright = string.IsNullOrWhiteSpace(siteSettings?.Copyright)
                    ? $"\u00a9 {DateTime.Now.Year} {siteSettings?.SiteName ?? "SCMS"}"
                    : siteSettings.Copyright,
                LogoUrl = siteSettings?.Logo ?? "/Themes/default/images/SCMS_Logo.png",
                FaviconPath = Path.Combine("/Themes", themeName, siteSettings?.Theme?.Favicon ?? "favicon.ico"),
                ThemeName = themeName,
                ThemePath = themePath,
                HttpContext = httpContext,
                SocialLinks = socialLinks,
                Services = httpContext?.RequestServices
            };

            // Phase 3: Run all token handlers in priority order
            var orderedHandlers = _tokenHandlers.OrderBy(h => h.Priority);

            foreach (var handler in orderedHandlers)
            {
                try
                {
                    if (handler.TokenPattern != null)
                    {
                        // Regex-based handler — replace each match
                        result = await ReplaceRegexAsync(result, handler.TokenPattern, async match =>
                            await handler.RenderAsync(match, renderContext));
                    }
                    else if (handler.SimpleToken != null && result.Contains(handler.SimpleToken))
                    {
                        // Simple string replacement
                        var replacement = await handler.RenderAsync(null, renderContext);
                        result = result.Replace(handler.SimpleToken, replacement);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Token handler '{HandlerName}' failed", handler.Name);
                }
            }

            // Phase 4: Inject Highlight.js (auto-injected, not a token)
            if (!result.Contains("atom-one-dark.min.css"))
            {
                result = result.Replace("</head>", @"
                <link href=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css"" rel=""stylesheet"">
                <link href=""/css/highlight-patch.css"" rel=""stylesheet"">
                </head>");
            }
            if (!result.Contains("highlight.min.js"))
            {
                result = result.Replace("</body>", @"
                <script src=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js""></script>
                <script>hljs.highlightAll();</script>
                </body>");
            }

            // Phase 5: Catch any unhandled tokens
            result = Regex.Replace(result, @"<cms:[^>]+\/>", match =>
            {
                var safeToken = match.Value.Replace("<", "(").Replace(">", ")");
                return $"<span style='color: red; font-weight: bold;'>[UNKNOWN TOKEN: {safeToken}]</span>";
            });

            return result;
        }

        /// <summary>
        /// Async regex replace — calls an async function for each match.
        /// </summary>
        private static async Task<string> ReplaceRegexAsync(string input, Regex regex, Func<Match, Task<string>> replacer)
        {
            var matches = regex.Matches(input);
            if (matches.Count == 0) return input;

            var sb = new System.Text.StringBuilder();
            var lastIndex = 0;

            foreach (Match match in matches)
            {
                sb.Append(input, lastIndex, match.Index - lastIndex);
                sb.Append(await replacer(match));
                lastIndex = match.Index + match.Length;
            }

            sb.Append(input, lastIndex, input.Length - lastIndex);
            return sb.ToString();
        }
    }
}
