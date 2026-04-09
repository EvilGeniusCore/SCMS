using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class SiteLogoTokenHandler : ITokenHandler
    {
        public string Name => "SiteLogo";
        public Regex? TokenPattern => new(@"<cms:SiteLogo(?:\s+height\s*=\s*""(?<height>\d+)"")?\s*\/>");
        public string? SimpleToken => null;
        public int Priority => 100;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var height = match?.Groups["height"].Success == true ? match.Groups["height"].Value : "50";
            var logoUrl = context.LogoUrl ?? "/Themes/default/images/SCMS_Logo.png";
            return Task.FromResult($"<img src=\"{logoUrl}\" alt=\"Site Logo\" style=\"max-height: {height}px;\">");
        }
    }
}
