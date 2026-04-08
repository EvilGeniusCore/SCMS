using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SCMS.Data;
using Microsoft.EntityFrameworkCore;
using SCMS.Interfaces;

namespace SCMS.Controllers
{
    [Route("")]
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IThemeEngine _themeEngine;
        private readonly ApplicationDbContext _db;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IThemeEngine themeEngine, ApplicationDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _themeEngine = themeEngine;
            _db = db;
        }

        [HttpGet("login")]
        public async Task<IActionResult> LoginPage()
        {
            var page = await _db.PageContents.FirstOrDefaultAsync(p => p.PageKey == "login");
            if (page == null) return NotFound();

            var html = await _themeEngine.RenderAsync(page, _db);
            return Content(html, "text/html");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginPost(string email, string password, bool rememberMe = false)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return Redirect("/");
                }
            }

            return Redirect("/login?error=true");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}
