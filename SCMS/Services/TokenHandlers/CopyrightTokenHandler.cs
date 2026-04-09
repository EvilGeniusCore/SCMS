using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class CopyrightTokenHandler : ITokenHandler
    {
        public string Name => "Copyright";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:Copyright />";
        public int Priority => 100;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
            => Task.FromResult(context.Copyright ?? $"\u00a9 {DateTime.Now.Year} {context.SiteName}");
    }
}
