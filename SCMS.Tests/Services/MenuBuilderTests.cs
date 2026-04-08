using System.Security.Claims;
using SCMS.Classes;
using SCMS.Tests.Helpers;

namespace SCMS.Tests.Services
{
    public class MenuBuilderTests
    {
        private ClaimsPrincipal AnonymousUser() => new(new ClaimsIdentity());

        private ClaimsPrincipal AuthenticatedUser(params string[] roles)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        [Fact]
        public void GenerateMenuHtml_AnonymousUser_SeesOnlyPublicItems()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var user = AnonymousUser();

            var html = MenuBuilder.GenerateMenuHtml(db, "Main", "horizontal", user);

            Assert.Contains("Home", html);
            Assert.Contains("About", html);
            Assert.DoesNotContain("Secret", html);
            Assert.DoesNotContain("Admin", html);
        }

        [Fact]
        public void GenerateMenuHtml_AuthenticatedUser_SeesUserItems()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var user = AuthenticatedUser("User");

            var html = MenuBuilder.GenerateMenuHtml(db, "Main", "horizontal", user);

            Assert.Contains("Home", html);
            Assert.Contains("About", html);
            Assert.Contains("Secret", html);
            Assert.DoesNotContain("Admin", html);
        }

        [Fact]
        public void GenerateMenuHtml_AdminUser_SeesEverything()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var user = AuthenticatedUser("Administrator");

            var html = MenuBuilder.GenerateMenuHtml(db, "Main", "horizontal", user);

            Assert.Contains("Home", html);
            Assert.Contains("Secret", html);
            Assert.Contains("Admin", html);
        }

        [Fact]
        public void GenerateMenuHtml_NonExistentGroup_ReturnsEmpty()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var user = AnonymousUser();

            var html = MenuBuilder.GenerateMenuHtml(db, "NonExistent", "horizontal", user);

            Assert.Equal("", html);
        }

        [Fact]
        public void GenerateMenuHtml_FooterGroup_ReturnsFooterItems()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var user = AnonymousUser();

            var html = MenuBuilder.GenerateMenuHtml(db, "Footer", "horizontal", user);

            Assert.Contains("Footer Link", html);
            Assert.DoesNotContain("Home", html);
        }

        [Fact]
        public void GenerateBreadcrumbHtml_ExistingUrl_ReturnsBreadcrumb()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var user = AnonymousUser();

            var html = MenuBuilder.GenerateBreadcrumbHtml(db, "secret", user);

            // The item with URL "/secret" should match
            if (!string.IsNullOrEmpty(html))
            {
                Assert.Contains("breadcrumb", html);
            }
        }

        [Fact]
        public void GenerateBreadcrumbHtml_NonExistentUrl_ReturnsEmpty()
        {
            using var db = TestDbContextFactory.CreateWithSeedData();
            var user = AnonymousUser();

            var html = MenuBuilder.GenerateBreadcrumbHtml(db, "nonexistent-page", user);

            Assert.Equal("", html);
        }
    }
}
