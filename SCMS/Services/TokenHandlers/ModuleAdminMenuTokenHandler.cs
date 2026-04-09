using System.Text;
using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    /// <summary>
    /// Renders a list of admin menu items contributed by loaded modules.
    /// Use <cms:ModuleAdminMenu /> in admin templates to render the module menu section.
    /// Only visible to authenticated admin users.
    /// </summary>
    public class ModuleAdminMenuTokenHandler : ITokenHandler
    {
        public string Name => "ModuleAdminMenu";
        public Regex? TokenPattern => new(@"<cms:ModuleAdminMenu\s*\/>", RegexOptions.IgnoreCase);
        public string? SimpleToken => null;
        public int Priority => 350;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var user = context.HttpContext?.User;
            if (user?.IsInRole("Administrator") != true)
                return Task.FromResult("");

            var modules = ModuleLoader.LoadedModules;
            if (!modules.Any())
                return Task.FromResult("");

            var sb = new StringBuilder();
            foreach (var module in modules)
            {
                foreach (var menuItem in module.GetAdminMenuItems())
                {
                    sb.Append($"<li><a class=\"dropdown-item\" href=\"{menuItem.Url}\"><i class=\"{menuItem.IconClass} me-2\"></i>{menuItem.Title}</a></li>");
                }
            }

            return Task.FromResult(sb.ToString());
        }
    }
}
