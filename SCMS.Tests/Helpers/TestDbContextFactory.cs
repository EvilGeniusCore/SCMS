using Microsoft.EntityFrameworkCore;
using SCMS.Data;

namespace SCMS.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext Create(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        public static ApplicationDbContext CreateWithSeedData(string? dbName = null)
        {
            var db = Create(dbName);

            // EnsureCreated runs OnModelCreating which inserts HasData seed.
            // That covers SecurityLevels, SecurityLevelRoles, ThemeSettings,
            // SiteSettings, PageContents (id 1-8), MenuItems, and roles.
            db.Database.EnsureCreated();

            // Add extra test-only data that doesn't conflict with HasData IDs.
            // HasData already seeds: PageContent ids 1-8, MenuItem ids 1-5,100
            // SecurityLevel ids 1-3, SecurityLevelRole ids 1-2, etc.

            // Add an "about" page (not in seed data)
            db.PageContents.Add(new PageContent
            {
                Id = 50,
                PageKey = "about",
                Title = "About",
                HtmlContent = "<p>About page</p>"
            });

            // Add extra menu items for testing
            db.MenuItems.AddRange(
                new MenuItem { Id = 101, Title = "About", PageContentId = 50, MenuGroup = "Main", Order = 1, IsVisible = true, SecurityLevelId = 3 },
                new MenuItem { Id = 102, Title = "Secret", MenuGroup = "Main", Order = 2, IsVisible = true, SecurityLevelId = 2, Url = "/secret" },
                new MenuItem { Id = 200, Title = "Footer Link", MenuGroup = "Footer", Order = 0, IsVisible = true, SecurityLevelId = 3, Url = "/footer" }
            );

            db.SaveChanges();
            return db;
        }
    }
}
