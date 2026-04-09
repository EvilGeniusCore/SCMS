using Microsoft.AspNetCore.Http;

namespace SCMS.Abstractions
{
    /// <summary>
    /// Shared context assembled once per render pass and handed to every ITokenHandler.
    /// Avoids duplicate DB queries across handlers.
    /// </summary>
    public class TokenRenderContext
    {
        /// <summary>Page being rendered.</summary>
        public required string PageTitle { get; init; }
        public required string PageContent { get; init; }
        public string? MetaDescription { get; init; }
        public string? MetaKeywords { get; init; }

        /// <summary>Site-level settings.</summary>
        public required string SiteName { get; init; }
        public string? Tagline { get; init; }
        public string? Copyright { get; init; }
        public string? LogoUrl { get; init; }
        public string? FaviconPath { get; init; }

        /// <summary>Theme info.</summary>
        public required string ThemeName { get; init; }
        public required string ThemePath { get; init; }

        /// <summary>Current HTTP context (user, request, services).</summary>
        public HttpContext? HttpContext { get; init; }

        /// <summary>Pre-loaded social links as dictionaries for template rendering.</summary>
        public List<Dictionary<string, object>> SocialLinks { get; init; } = new();

        /// <summary>
        /// The IServiceProvider for resolving scoped services (DbContext etc.)
        /// Handlers should use this rather than HttpContext.RequestServices where possible.
        /// </summary>
        public IServiceProvider? Services { get; init; }
    }
}
