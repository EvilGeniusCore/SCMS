using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.Data;
using SCMS.Interfaces;
using SCMS.Services;

namespace SCMS.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    [Route("admin/[controller]")]
    public class ModulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RazorRenderer _razorRenderer;
        private readonly IThemeEngine _themeEngine;

        public ModulesController(ApplicationDbContext context, RazorRenderer razorRenderer, IThemeEngine themeEngine)
        {
            _context = context;
            _razorRenderer = razorRenderer;
            _themeEngine = themeEngine;
        }

        [HttpGet("/admin/modules")]
        public async Task<IActionResult> Index()
        {
            var modules = ModuleLoader.LoadedModules;

            var html = "<div class=\"container mt-5\">"
                + "<h2 class=\"mb-4\">Loaded Modules</h2>";

            if (!modules.Any())
            {
                html += "<p class=\"text-muted\">No modules loaded. Drop module DLLs into the <code>/Modules</code> folder and restart.</p>";
            }
            else
            {
                html += "<table class=\"table table-striped\"><thead><tr>"
                    + "<th>Module</th><th>Version</th><th>Description</th><th>Admin Pages</th>"
                    + "</tr></thead><tbody>";

                foreach (var module in modules)
                {
                    var adminLinks = string.Join(", ",
                        module.GetAdminMenuItems().Select(m =>
                            $"<a href=\"{m.Url}\">{m.Title}</a>"));

                    html += $"<tr>"
                        + $"<td><strong>{module.Name}</strong></td>"
                        + $"<td>{module.Version}</td>"
                        + $"<td>{module.Description}</td>"
                        + $"<td>{(string.IsNullOrEmpty(adminLinks) ? "—" : adminLinks)}</td>"
                        + "</tr>";
                }

                html += "</tbody></table>";
            }

            html += "</div>";

            var wrapped = await _themeEngine.RenderAsync(new PageContent
            {
                Title = "Modules",
                HtmlContent = html
            }, _context);

            return Content(wrapped, "text/html");
        }
    }
}
