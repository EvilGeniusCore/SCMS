using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class FaviconTokenHandler : ITokenHandler
    {
        public string Name => "Favicon";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:Favicon />";
        public int Priority => 50;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
            => Task.FromResult($"<link rel=\"icon\" href=\"{context.FaviconPath}\" type=\"image/x-icon\">");
    }
}
