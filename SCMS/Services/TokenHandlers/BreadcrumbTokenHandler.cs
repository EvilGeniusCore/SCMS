using System.Security.Claims;
using System.Text.RegularExpressions;
using SCMS.Abstractions;
using SCMS.Classes;
using SCMS.Data;

namespace SCMS.Services.TokenHandlers
{
    public class BreadcrumbTokenHandler : ITokenHandler
    {
        public string Name => "Breadcrumb";
        public Regex? TokenPattern => new(@"<cms:Breadcrumb\s*\/>", RegexOptions.IgnoreCase);
        public string? SimpleToken => null;
        public int Priority => 300;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var httpContext = context.HttpContext;
            var requestPath = httpContext?.Request.Path.Value?.Trim('/') ?? "";
            var principal = httpContext?.User ?? new ClaimsPrincipal();

            var db = context.Services?.GetService<ApplicationDbContext>();
            if (db == null) return Task.FromResult("");

            var html = MenuBuilder.GenerateBreadcrumbHtml(db, requestPath, principal);

            return Task.FromResult(string.IsNullOrWhiteSpace(html)
                ? "<nav aria-label=\"breadcrumb\"><ol class=\"breadcrumb mb-0\"><li class=\"breadcrumb-item active\" aria-current=\"page\">Home</li></ol></nav>"
                : html);
        }
    }
}
