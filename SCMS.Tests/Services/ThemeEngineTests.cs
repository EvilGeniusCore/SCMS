using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SCMS.Abstractions;
using SCMS.Data;
using SCMS.Interfaces;
using SCMS.Services;
using SCMS.Services.Theme;
using SCMS.Services.TokenHandlers;
using SCMS.Tests.Helpers;

namespace SCMS.Tests.Services
{
    [Collection("Sequential")]
    public class ThemeEngineTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _originalDir;
        private readonly ApplicationDbContext _db;
        private readonly ThemeEngine _engine;

        private static IEnumerable<ITokenHandler> AllBuiltInHandlers() => new ITokenHandler[]
        {
            new PageTitleTokenHandler(),
            new ContentTokenHandler(),
            new FaviconTokenHandler(),
            new MetaTagsTokenHandler(),
            new CopyrightTokenHandler(),
            new TaglineTokenHandler(),
            new LoginStatusTokenHandler(),
            new UserNameTokenHandler(),
            new ErrorMessageTokenHandler(),
            new SiteNameTokenHandler(),
            new SiteLogoTokenHandler(),
            new MenuTokenHandler(),
            new AntiforgeryTokenHandler(),
            new SocialLinksTokenHandler(),
            new BreadcrumbTokenHandler()
        };

        public ThemeEngineTests()
        {
            _originalDir = Directory.GetCurrentDirectory();

            _tempDir = Path.Combine(Path.GetTempPath(), "scms_test_" + Guid.NewGuid().ToString("N"));
            var themePath = Path.Combine(_tempDir, "Themes", "default");
            Directory.CreateDirectory(Path.Combine(themePath, "templates"));
            Directory.CreateDirectory(Path.Combine(themePath, "partials"));

            File.WriteAllText(Path.Combine(themePath, "layout.html"),
                "<html><head><title><cms:PageTitle /></title></head><body><cms:Header /><cms:Content /><cms:Footer /></body></html>");
            File.WriteAllText(Path.Combine(themePath, "templates", "page.html"),
                "<article><cms:ErrorMessage /><cms:Content /></article>");
            File.WriteAllText(Path.Combine(themePath, "partials", "header.html"),
                "<header><cms:SiteName /> - <cms:LoginStatus /></header>");
            File.WriteAllText(Path.Combine(themePath, "partials", "footer.html"),
                "<footer><cms:Copyright /> <cms:Tagline /></footer>");

            Directory.SetCurrentDirectory(_tempDir);

            _db = TestDbContextFactory.CreateWithSeedData();

            var services = new ServiceCollection();
            services.AddLogging();
            var sp = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext { RequestServices = sp };
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var themeManager = new Mock<IThemeManager>();
            themeManager.Setup(t => t.GetCurrentThemeAsync(It.IsAny<ApplicationDbContext>()))
                .ReturnsAsync("default");

            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = new Mock<ILogger<ThemeEngine>>();

            _engine = new ThemeEngine(httpContextAccessor.Object, themeManager.Object, cache, logger.Object, AllBuiltInHandlers());
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_originalDir);
            _db.Dispose();
            try { Directory.Delete(_tempDir, true); } catch { }
        }

        [Fact]
        public async Task RenderAsync_ReplacesPageTitle()
        {
            var page = new PageContent { Title = "Test Page", HtmlContent = "<p>Hello</p>" };
            var result = await _engine.RenderAsync(page, _db);
            Assert.Contains("<title>Test Page</title>", result);
        }

        [Fact]
        public async Task RenderAsync_ReplacesSiteName()
        {
            var page = new PageContent { Title = "Test", HtmlContent = "" };
            var result = await _engine.RenderAsync(page, _db);
            Assert.Contains("SCMS Site", result);
        }

        [Fact]
        public async Task RenderAsync_InjectsPageContent()
        {
            var page = new PageContent { Title = "Test", HtmlContent = "<p>My content here</p>" };
            var result = await _engine.RenderAsync(page, _db);
            Assert.Contains("My content here", result);
        }

        [Fact]
        public async Task RenderAsync_AnonymousUser_ShowsLoginLink()
        {
            var page = new PageContent { Title = "Test", HtmlContent = "" };
            var result = await _engine.RenderAsync(page, _db);
            Assert.Contains("Login", result);
            Assert.Contains("portal-access", result);
        }

        [Fact]
        public async Task RenderAsync_IncludesHeaderAndFooter()
        {
            var page = new PageContent { Title = "Test", HtmlContent = "" };
            var result = await _engine.RenderAsync(page, _db);
            Assert.Contains("<header>", result);
            Assert.Contains("<footer>", result);
        }

        [Fact]
        public async Task RenderAsync_ReplacesTagline()
        {
            var page = new PageContent { Title = "Test", HtmlContent = "" };
            var result = await _engine.RenderAsync(page, _db);
            Assert.Contains("Powered by SCMS", result);
        }

        [Fact]
        public async Task RenderAsync_UnknownToken_MarkedAsUnknown()
        {
            var layoutPath = Path.Combine(_tempDir, "Themes", "default", "layout.html");
            var layout = File.ReadAllText(layoutPath);
            layout = layout.Replace("</body>", "<cms:UnknownWidget /></body>");
            File.WriteAllText(layoutPath, layout);

            var sp2 = new ServiceCollection().AddLogging().BuildServiceProvider();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { RequestServices = sp2 });
            var themeManager = new Mock<IThemeManager>();
            themeManager.Setup(t => t.GetCurrentThemeAsync(It.IsAny<ApplicationDbContext>())).ReturnsAsync("default");
            var freshEngine = new ThemeEngine(httpContextAccessor.Object, themeManager.Object,
                new MemoryCache(new MemoryCacheOptions()), new Mock<ILogger<ThemeEngine>>().Object, AllBuiltInHandlers());

            var page = new PageContent { Title = "Test", HtmlContent = "" };
            var result = await freshEngine.RenderAsync(page, _db);

            Assert.Contains("UNKNOWN TOKEN", result);
        }

        [Fact]
        public async Task RenderAsync_MissingThemeDir_ReturnsFallbackError()
        {
            var sp3 = new ServiceCollection().AddLogging().BuildServiceProvider();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { RequestServices = sp3 });
            var themeManager = new Mock<IThemeManager>();
            themeManager.Setup(t => t.GetCurrentThemeAsync(It.IsAny<ApplicationDbContext>())).ReturnsAsync("nonexistent_theme");
            var engine = new ThemeEngine(httpContextAccessor.Object, themeManager.Object,
                new MemoryCache(new MemoryCacheOptions()), new Mock<ILogger<ThemeEngine>>().Object, AllBuiltInHandlers());

            var page = new PageContent { Title = "Test", HtmlContent = "<p>Content</p>" };
            var result = await engine.RenderAsync(page, _db);

            Assert.Contains("Theme Error", result);
            Assert.Contains("Content", result);
        }
    }
}
