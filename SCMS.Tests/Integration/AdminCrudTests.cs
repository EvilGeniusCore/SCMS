using SCMS.Data;
using SCMS.Models;
using Microsoft.Extensions.DependencyInjection;

namespace SCMS.Tests.Integration
{
    [Collection("Sequential")]
    public class AdminCrudTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AdminCrudTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task MenuService_CreateAndDelete_ViaDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var menuService = new SCMS.Services.MenuService(db);

            await menuService.CreateGroupAsync("IntTestGroup");
            var items = db.MenuItems.Where(m => m.MenuGroup == "IntTestGroup").ToList();
            Assert.Single(items);

            await menuService.CreateMenuItemAsync(new CreateItemModel
            {
                Title = "Int Test Item",
                Group = "IntTestGroup"
            });
            items = db.MenuItems.Where(m => m.MenuGroup == "IntTestGroup").ToList();
            Assert.Equal(2, items.Count);

            var deleted = await menuService.DeleteGroupAsync("IntTestGroup");
            Assert.True(deleted);
            Assert.Empty(db.MenuItems.Where(m => m.MenuGroup == "IntTestGroup"));
        }

        [Fact]
        public async Task PageContentService_SaveAndVerify_ViaDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            var pageService = new SCMS.Services.PageContentService(db, env);

            // Use seed item id=100 (Home, page=1)
            var model = new MenuItemUpdateModel
            {
                Id = 100,
                Title = "IntTest Home",
                IsExternal = false,
                IsVisible = true,
                SecurityLevelId = 3,
                HtmlContent = "<p>Integration test update</p>",
                PageTitle = "IntTest Title"
            };

            var result = await pageService.SaveMenuItemContentAsync(model);
            Assert.True(result);

            var page = db.PageContents.First(p => p.Id == 1);
            Assert.Equal("IntTest Title", page.Title);
            Assert.Contains("Integration test update", page.HtmlContent);
        }

        [Fact]
        public async Task MenuService_Reorder_AdminPinnedToEnd()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var menuService = new SCMS.Services.MenuService(db);

            await menuService.ReorderAsync(new List<ReorderItem>
            {
                new() { Id = 1, Order = 0 },
                new() { Id = 100, Order = 1 }
            });

            var admin = db.MenuItems.First(m => m.Id == 1);
            var home = db.MenuItems.First(m => m.Id == 100);
            Assert.Equal(9999, admin.Order);
            Assert.Equal(1, home.Order);
        }

        [Fact]
        public async Task NavContent_Load_Unauthenticated_Redirects()
        {
            var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync("/admin/navcontent/load/100");

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        }
    }
}
