using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class LoginStatusTokenHandler : ITokenHandler
    {
        public string Name => "LoginStatus";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:LoginStatus />";
        public int Priority => 200;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var isAuthenticated = context.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            var html = isAuthenticated
                ? "<a href=\"/portal-logout\">Logout</a>"
                : "<a href=\"/portal-access\">Login</a>";
            return Task.FromResult(html);
        }
    }
}
