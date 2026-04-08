using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SCMS.Interfaces;
using SCMS.Data;

namespace SCMS.Controllers
{
    public class PageController : Controller
    {
        private readonly IPageService _pageService;
        private readonly IThemeEngine _themeEngine;
        private readonly ApplicationDbContext _db;

        public PageController(IPageService pageService, IThemeEngine themeEngine, ApplicationDbContext db)
        {
            _pageService = pageService;
            _themeEngine = themeEngine;
            _db = db;
        }

        [Route("{slug?}")]
        public async Task<IActionResult> RenderPage(string? slug = "home")
        {
            var page = await _pageService.GetPageBySlugAsync(slug ?? "home");
            if (page == null) return NotFound();

            var html = await _themeEngine.RenderAsync(page, _db);
            return Content(html, "text/html");
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> ChangePasswordPost(string Input_OldPassword, string Input_NewPassword, string Input_ConfirmPassword)
        {
            if (Input_NewPassword != Input_ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return Redirect("/change-password");
            }

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var signInManager = HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/login");
            }

            var result = await userManager.ChangePasswordAsync(user, Input_OldPassword, Input_NewPassword);
            if (result.Succeeded)
            {
                user.MustChangePassword = false;
                await userManager.UpdateAsync(user);
                await signInManager.SignOutAsync();
                return Redirect("/login");
            }

            TempData["Error"] = "Password change failed: " + string.Join(" ", result.Errors.Select(e => e.Description));
            return Redirect("/change-password");
        }

        [HttpGet]
        [Route("portal-logout")]
        public async Task<IActionResult> Logout()
        {
            var signInManager = HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            await signInManager.SignOutAsync();

            // 🔁 Instead of redirecting, fall through to page renderer
            var page = await _pageService.GetPageBySlugAsync("portal-logout");
            if (page == null) return NotFound();

            var html = await _themeEngine.RenderAsync(page, _db);
            return Content(html, "text/html");
        }
    }
}
