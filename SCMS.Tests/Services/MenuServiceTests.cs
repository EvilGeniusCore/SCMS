using SCMS.Data;
using SCMS.Models;
using SCMS.Services;
using SCMS.Tests.Helpers;

namespace SCMS.Tests.Services
{
    public class MenuServiceTests
    {
        private (ApplicationDbContext db, MenuService service) CreateService()
        {
            var db = TestDbContextFactory.CreateWithSeedData();
            return (db, new MenuService(db));
        }

        [Fact]
        public async Task GetAllMenuItemsWithIncludesAsync_ReturnsAllItems()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var items = await service.GetAllMenuItemsWithIncludesAsync();
                // Seed: 100,1,2,3,4,5 + test: 101,102,200 = 9
                Assert.True(items.Count >= 9);
            }
        }

        [Fact]
        public async Task GetMenuItemAsync_ExistingId_ReturnsItem()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var item = await service.GetMenuItemAsync(100);
                Assert.NotNull(item);
                Assert.Equal("Home", item.Title);
                Assert.NotNull(item.PageContent);
            }
        }

        [Fact]
        public async Task GetMenuItemAsync_NonExistentId_ReturnsNull()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var item = await service.GetMenuItemAsync(9999);
                Assert.Null(item);
            }
        }

        [Fact]
        public async Task GetGroupItemsAsync_ReturnsOnlyGroupItems_ExcludingAdmin()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var items = await service.GetGroupItemsAsync("Main");
                Assert.All(items, i => Assert.NotEqual("Admin", i.Title));
                Assert.Contains(items, i => i.Title == "Home");
            }
        }

        [Fact]
        public async Task GetGroupItemsAsync_NonExistentGroup_ReturnsEmpty()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var items = await service.GetGroupItemsAsync("NonExistent");
                Assert.Empty(items);
            }
        }

        [Fact]
        public async Task CreateGroupAsync_CreatesGroupWithMenuItem()
        {
            var (db, service) = CreateService();
            using (db)
            {
                await service.CreateGroupAsync("Sidebar");
                var items = db.MenuItems.Where(m => m.MenuGroup == "Sidebar").ToList();
                Assert.Single(items);
                Assert.Equal("New Menu Item", items[0].Title);
            }
        }

        [Fact]
        public async Task RenameGroupAsync_ExistingGroup_RenamesAll()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var result = await service.RenameGroupAsync("Footer", "BottomNav");
                Assert.True(result);
                Assert.Empty(db.MenuItems.Where(m => m.MenuGroup == "Footer"));
                Assert.Single(db.MenuItems.Where(m => m.MenuGroup == "BottomNav"));
            }
        }

        [Fact]
        public async Task RenameGroupAsync_NonExistentGroup_ReturnsFalse()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var result = await service.RenameGroupAsync("NoSuchGroup", "NewName");
                Assert.False(result);
            }
        }

        [Fact]
        public async Task DeleteGroupAsync_ExistingGroup_RemovesItemsAndPages()
        {
            var (db, service) = CreateService();
            using (db)
            {
                await service.CreateGroupAsync("Temp");
                Assert.Single(db.MenuItems.Where(m => m.MenuGroup == "Temp"));

                var result = await service.DeleteGroupAsync("Temp");
                Assert.True(result);
                Assert.Empty(db.MenuItems.Where(m => m.MenuGroup == "Temp"));
            }
        }

        [Fact]
        public async Task DeleteGroupAsync_NonExistentGroup_ReturnsFalse()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var result = await service.DeleteGroupAsync("NoSuchGroup");
                Assert.False(result);
            }
        }

        [Fact]
        public async Task CreateMenuItemAsync_InsertsItemWithPage()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var countBefore = db.MenuItems.Count();
                await service.CreateMenuItemAsync(new CreateItemModel
                {
                    Title = "New Page",
                    Group = "Main"
                });

                Assert.Equal(countBefore + 1, db.MenuItems.Count());
                var created = db.MenuItems.First(m => m.Title == "New Page");
                Assert.NotNull(created.PageContentId);
            }
        }

        [Fact]
        public async Task DeleteMenuItemAsync_ExistingItem_RemovesItemAndPage()
        {
            var (db, service) = CreateService();
            using (db)
            {
                // Use the test-added About item (id=101, page=50)
                var result = await service.DeleteMenuItemAsync(101);
                Assert.True(result);
                Assert.Null(db.MenuItems.FirstOrDefault(m => m.Id == 101));
                Assert.Null(db.PageContents.FirstOrDefault(p => p.Id == 50));
            }
        }

        [Fact]
        public async Task DeleteMenuItemAsync_NonExistentId_ReturnsFalse()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var result = await service.DeleteMenuItemAsync(9999);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task SetParentAsync_SetsParentId()
        {
            var (db, service) = CreateService();
            using (db)
            {
                await service.SetParentAsync(101, 100);
                var item = db.MenuItems.First(m => m.Id == 101);
                Assert.Equal(100, item.ParentId);
            }
        }

        [Fact]
        public async Task ReorderAsync_AdminItem_ForcedTo9999()
        {
            var (db, service) = CreateService();
            using (db)
            {
                await service.ReorderAsync(new List<ReorderItem>
                {
                    new() { Id = 1, Order = 0 }
                });
                var admin = db.MenuItems.First(m => m.Id == 1);
                Assert.Equal(9999, admin.Order);
            }
        }

        [Fact]
        public async Task ReorderAsync_NormalItem_SetsOrder()
        {
            var (db, service) = CreateService();
            using (db)
            {
                await service.ReorderAsync(new List<ReorderItem>
                {
                    new() { Id = 100, Order = 5 }
                });
                var item = db.MenuItems.First(m => m.Id == 100);
                Assert.Equal(5, item.Order);
            }
        }

        [Fact]
        public void GetAdminNodeIds_ReturnsAdminAndChildren()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var allItems = db.MenuItems.ToList();
                var adminIds = service.GetAdminNodeIds(allItems);

                Assert.Contains(1, adminIds);  // Admin root
                Assert.Contains(2, adminIds);  // Settings (child)
                Assert.Contains(3, adminIds);  // Social Media (child)
                Assert.Contains(4, adminIds);  // Menu/Page Editor (child)
                Assert.DoesNotContain(100, adminIds); // Home
            }
        }

        [Fact]
        public void GetAdminNodeIds_NoAdminNode_ReturnsEmpty()
        {
            var service = new MenuService(TestDbContextFactory.Create());
            var items = new List<MenuItem>
            {
                new() { Id = 10, Title = "Home" },
                new() { Id = 11, Title = "About" }
            };
            var result = service.GetAdminNodeIds(items);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGroupStructureAsync_ReturnsFlattenedHierarchy()
        {
            var (db, service) = CreateService();
            using (db)
            {
                var structure = await service.GetGroupStructureAsync("Main");
                Assert.NotEmpty(structure);
            }
        }
    }
}
