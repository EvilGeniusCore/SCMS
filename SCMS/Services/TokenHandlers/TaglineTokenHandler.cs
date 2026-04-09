using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class TaglineTokenHandler : ITokenHandler
    {
        public string Name => "Tagline";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:Tagline />";
        public int Priority => 100;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
            => Task.FromResult(context.Tagline ?? "Site Powered by SCMS");
    }
}
