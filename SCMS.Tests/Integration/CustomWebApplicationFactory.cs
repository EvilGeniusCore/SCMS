using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCMS.Data;

namespace SCMS.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private static readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var webProjectDir = Path.Combine(solutionDir, "SCMS");
            builder.UseContentRoot(webProjectDir);

            builder.ConfigureServices(services =>
            {
                // Remove ALL DbContext registrations (options + context)
                var dbDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                             || d.ServiceType == typeof(ApplicationDbContext)
                             || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();
                foreach (var d in dbDescriptors) services.Remove(d);

                // Re-add with InMemory provider — shared DB name so seed persists
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });

            builder.UseEnvironment("Development");
        }
    }
}
