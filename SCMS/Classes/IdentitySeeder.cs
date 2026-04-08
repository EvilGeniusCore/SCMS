using Microsoft.AspNetCore.Identity;
using SCMS.Data;

namespace SCMS.Classes
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

            string adminEmail = configuration["AdminEmail"] ?? Environment.GetEnvironmentVariable("AdminEmail") ?? "admin@example.com";
            string adminPassword = configuration["AdminPassword"] ?? Environment.GetEnvironmentVariable("AdminPassword") ?? "P@ssword1";
            string adminUsername = configuration["AdminUsername"] ?? Environment.GetEnvironmentVariable("AdminUsername") ?? "AdminUser";

            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                await roleManager.CreateAsync(new IdentityRole("Administrator"));
            }

            bool isDefaultPassword = adminPassword == "P@ssword1";
            bool isDefaultEmail = adminEmail == "admin@example.com";
            bool isDefaultUsername = adminUsername == "Admin";

            bool mustChangePassword = isDefaultPassword || isDefaultEmail || isDefaultUsername;

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    MustChangePassword = mustChangePassword
                };

                var passwordValidator = serviceProvider.GetRequiredService<IPasswordValidator<ApplicationUser>>();
                var passwordResult = await passwordValidator.ValidateAsync(userManager, user, adminPassword);

                var userValidators = serviceProvider.GetServices<IUserValidator<ApplicationUser>>();
                var userErrors = new List<IdentityError>();

                foreach (var validator in userValidators)
                {
                    var vresult = await validator.ValidateAsync(userManager, user);
                    if (!vresult.Succeeded)
                        userErrors.AddRange(vresult.Errors);
                }

                if (!passwordResult.Succeeded || userErrors.Any())
                {
                    var allErrors = passwordResult.Errors.Concat(userErrors);
                    var message = string.Join(", ", allErrors.Select(e => e.Description));
                    throw new Exception($"[Seeder] Admin user validation failed: {message}");
                }

                try
                {
                    var result = await userManager.CreateAsync(user, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Administrator");
                        logger.LogInformation("Admin user '{Email}' created successfully", adminEmail);
                    }
                    else
                    {
                        var message = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"[Seeder] Admin user creation failed: {message}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception during admin user creation");
                    throw;
                }
            }
        }
    }
}
