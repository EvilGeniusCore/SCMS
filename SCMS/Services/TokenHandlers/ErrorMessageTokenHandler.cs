using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class ErrorMessageTokenHandler : ITokenHandler
    {
        public string Name => "ErrorMessage";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "<cms:ErrorMessage />";
        public int Priority => 200;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var httpContext = context.HttpContext;
            var tempData = httpContext?.RequestServices
                .GetService<ITempDataDictionaryFactory>()
                ?.GetTempData(httpContext);

            var err = tempData?["Error"] as string;
            var html = !string.IsNullOrEmpty(err)
                ? $"<div class='alert alert-danger'>{err}</div>"
                : "";
            return Task.FromResult(html);
        }
    }
}
