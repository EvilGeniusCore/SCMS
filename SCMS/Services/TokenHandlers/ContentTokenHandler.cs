using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class ContentTokenHandler : ITokenHandler
    {
        public string Name => "Content";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:Content />";
        public int Priority => 20;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
            => Task.FromResult(context.PageContent);
    }
}
