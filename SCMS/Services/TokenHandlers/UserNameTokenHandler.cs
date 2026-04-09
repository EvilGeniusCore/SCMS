using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class UserNameTokenHandler : ITokenHandler
    {
        public string Name => "UserName";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:UserName />";
        public int Priority => 200;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var user = context.HttpContext?.User;
            var displayName = user?.Identity?.IsAuthenticated == true
                ? (user.Identity?.Name ?? "User")
                : "Guest";
            return Task.FromResult(displayName);
        }
    }
}
