using System.Text.RegularExpressions;
using SCMS.Abstractions;
using SCMS.Services.Template;

namespace SCMS.Services.TokenHandlers
{
    public class SocialLinksTokenHandler : ITokenHandler
    {
        public string Name => "SocialLinks";
        public Regex? TokenPattern => new(@"<cms:SocialLinks\s*\/>", RegexOptions.IgnoreCase);
        public string? SimpleToken => null;
        public int Priority => 300;

        public async Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var templatePath = Path.Combine(context.ThemePath, "partials", "social.template.html");
            if (!File.Exists(templatePath))
                return "";

            var templateText = await File.ReadAllTextAsync(templatePath);

            var socialData = new Dictionary<string, object>
            {
                ["Items"] = context.SocialLinks.Cast<object>().ToList()
            };

            var parser = new TemplateParser();
            return parser.Parse(templateText, socialData);
        }
    }
}
