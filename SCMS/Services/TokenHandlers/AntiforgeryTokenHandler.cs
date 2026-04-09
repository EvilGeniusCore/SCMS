using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Antiforgery;
using SCMS.Abstractions;

namespace SCMS.Services.TokenHandlers
{
    public class AntiforgeryTokenHandler : ITokenHandler
    {
        public string Name => "AntiforgeryToken";
        public Regex? TokenPattern => null;
        public string? SimpleToken => "{{ANTIFORGERY_TOKEN}}";
        public int Priority => 400;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            var httpContext = context.HttpContext;
            var antiforgery = httpContext?.RequestServices.GetService<IAntiforgery>();
            var tokenValue = "";
            if (antiforgery != null && httpContext != null)
            {
                var tokenSet = antiforgery.GetAndStoreTokens(httpContext);
                tokenValue = tokenSet.RequestToken ?? "";
            }
            return Task.FromResult(tokenValue);
        }
    }
}
