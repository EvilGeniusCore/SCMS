using SCMS.Services;
using SCMS.Tests.Helpers;

namespace SCMS.Tests.Services
{
    public class PageServiceTests
    {
        [Fact]
        public async Task GetPageBySlugAsync_ExistingSlug_ReturnsPage()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var service = new PageService(db);

            var page = await service.GetPageBySlugAsync("home");

            Assert.NotNull(page);
            Assert.Equal("Welcome", page.Title);
        }

        [Fact]
        public async Task GetPageBySlugAsync_NonExistentSlug_ReturnsNull()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var service = new PageService(db);

            var page = await service.GetPageBySlugAsync("does-not-exist");

            Assert.Null(page);
        }

        [Fact]
        public async Task GetPageBySlugAsync_CaseSensitive_NoMatch()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var service = new PageService(db);

            // "home" exists but "Home" should not match (case-sensitive)
            var page = await service.GetPageBySlugAsync("Home");

            Assert.Null(page);
        }

        [Fact]
        public async Task GetPageBySlugAsync_ReturnsCorrectContent()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var service = new PageService(db);

            var page = await service.GetPageBySlugAsync("about");

            Assert.NotNull(page);
            Assert.Equal("About", page.Title);
            Assert.Contains("About page", page.HtmlContent);
        }
    }
}
