using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class SiteNameTokenHandler : ITokenHandler
    {
        public string Name => "SiteName";
        public Regex? TokenPattern => new(@"<cms:SiteName\s*\/>", RegexOptions.IgnoreCase);
        public string? SimpleToken => null;
        public int Priority => 100;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
            => Task.FromResult(context.SiteName);
    }
}
