using Microsoft.AspNetCore.Hosting;
using Moq;
using SCMS.Models;
using SCMS.Services;
using SCMS.Tests.Helpers;

namespace SCMS.Tests.Services
{
    public class PageContentServiceTests
    {
        private (SCMS.Data.ApplicationDbContext db, PageContentService service) CreateService()
        {
            var db = TestDbContextFactory.CreateWithSeedData();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            env.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
            return (db, new PageContentService(db, env.Object));
        }

        [Fact]
        public async Task SaveMenuItemContentAsync_UpdatesExistingPage()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var model = new MenuItemUpdateModel
                {
                    Id = 100, // Home (seed data)
                    Title = "Updated Home",
                    IsExternal = false,
                    IsVisible = true,
                    SecurityLevelId = 3,
                    HtmlContent = "<p>Updated content</p>",
                    PageTitle = "Updated Home Page"
                };

                var result = await service.SaveMenuItemContentAsync(model);

                Assert.True(result);
                var item = db.MenuItems.First(m => m.Id == 100);
                Assert.Equal("Updated Home", item.Title);
                var page = db.PageContents.First(p => p.Id == item.PageContentId);
                Assert.Equal("Updated Home Page", page.Title);
                Assert.Contains("Updated content", page.HtmlContent);
            }
        }

        [Fact]
        public async Task SaveMenuItemContentAsync_NonExistentItem_ReturnsFalse()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var model = new MenuItemUpdateModel { Id = 9999, Title = "Nope" };
                var result = await service.SaveMenuItemContentAsync(model);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task SaveMenuItemContentAsync_ExternalLink_RemovesPageContent()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var model = new MenuItemUpdateModel
                {
                    Id = 100,
                    Title = "Google",
                    IsExternal = true,
                    Url = "https://google.com",
                    IsVisible = true,
                    SecurityLevelId = 3
                };

                var result = await service.SaveMenuItemContentAsync(model);

                Assert.True(result);
                var item = db.MenuItems.First(m => m.Id == 100);
                Assert.Equal("https://google.com", item.Url);
                Assert.Null(item.PageContentId);
            }
        }

        [Fact]
        public async Task SaveMenuItemContentAsync_SanitizesXss()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var model = new MenuItemUpdateModel
                {
                    Id = 100,
                    Title = "Home",
                    IsExternal = false,
                    IsVisible = true,
                    SecurityLevelId = 3,
                    HtmlContent = "<p>Safe</p><script>alert('xss')</script>",
                    PageTitle = "Home"
                };

                await service.SaveMenuItemContentAsync(model);

                var page = db.PageContents.First(p => p.Id == db.MenuItems.First(m => m.Id == 100).PageContentId);
                Assert.Contains("Safe", page.HtmlContent);
                Assert.DoesNotContain("<script>", page.HtmlContent);
            }
        }

        [Fact]
        public async Task SaveMenuItemContentAsync_PreservesAllowedHtml()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var model = new MenuItemUpdateModel
                {
                    Id = 100,
                    Title = "Home",
                    IsExternal = false,
                    IsVisible = true,
                    SecurityLevelId = 3,
                    HtmlContent = "<p class=\"lead\">Hello</p><iframe src=\"https://youtube.com\"></iframe>",
                    PageTitle = "Home"
                };

                await service.SaveMenuItemContentAsync(model);

                var page = db.PageContents.First(p => p.Id == db.MenuItems.First(m => m.Id == 100).PageContentId);
                Assert.Contains("class=\"lead\"", page.HtmlContent);
                Assert.Contains("<iframe", page.HtmlContent);
            }
        }

        [Fact]
        public async Task SaveMenuItemContentAsync_CreatesPageWhenNoneExists()
        {
            var (db, service) = CreateService();
            using (db)
            {
                // Item 102 (Secret) has no PageContent, only a URL
                var model = new MenuItemUpdateModel
                {
                    Id = 102,
                    Title = "Secret",
                    IsExternal = false,
                    IsVisible = true,
                    SecurityLevelId = 2,
                    HtmlContent = "<p>Secret content</p>",
                    PageTitle = "Secret Page"
                };

                var result = await service.SaveMenuItemContentAsync(model);

                Assert.True(result);
                var updated = db.MenuItems.First(m => m.Id == 102);
                Assert.NotNull(updated.PageContentId);
            }
        }

        [Fact]
        public async Task SaveMenuItemContentAsync_MetaKeywords_JoinedCorrectly()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var model = new MenuItemUpdateModel
                {
                    Id = 100,
                    Title = "Home",
                    IsExternal = false,
                    IsVisible = true,
                    SecurityLevelId = 3,
                    HtmlContent = "<p>Content</p>",
                    PageTitle = "Home",
                    MetaDescription = "A test page",
                    MetaKeywords = new List<string> { "test", "home", "scms" }
                };

                await service.SaveMenuItemContentAsync(model);

                var page = db.PageContents.First(p => p.Id == db.MenuItems.First(m => m.Id == 100).PageContentId);
                Assert.Equal("test, home, scms", page.MetaKeywords);
                Assert.Equal("A test page", page.MetaDescription);
            }
        }
    }
}
