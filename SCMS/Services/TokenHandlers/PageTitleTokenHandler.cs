using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class PageTitleTokenHandler : ITokenHandler
    {
        public string Name => "PageTitle";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:PageTitle />";
        public int Priority => 10;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
            => Task.FromResult(context.PageTitle);
    }
}
