using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class MetaTagsTokenHandler : ITokenHandler
    {
        public string Name => "MetaTags";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:MetaTags />";
        public int Priority => 55;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(context.MetaDescription))
                sb.AppendLine($"    <meta name=\"description\" content=\"{WebUtility.HtmlEncode(context.MetaDescription)}\">");
            if (!string.IsNullOrWhiteSpace(context.MetaKeywords))
                sb.AppendLine($"    <meta name=\"keywords\" content=\"{WebUtility.HtmlEncode(context.MetaKeywords)}\">");
            return Task.FromResult(sb.ToString());
        }
    }
}
