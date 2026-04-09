using System.Text.RegularExpressions;

namespace SCMS.Abstractions
{
    /// <summary>
    /// Processes a single CMS token type (e.g. &lt;cms:Menu /&gt;, &lt;cms:SiteName /&gt;).
    /// ThemeEngine collects all registered handlers and runs them in Priority order.
    /// </summary>
    public interface ITokenHandler
    {
        /// <summary>
        /// Display name for logging/debugging (e.g. "Menu", "SiteName").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Regex pattern that matches this handler's token(s) in the rendered HTML.
        /// For simple tokens, match the exact tag. For tokens with attributes, capture groups.
        /// Return null to use simple string replacement via <see cref="SimpleToken"/> instead.
        /// </summary>
        Regex? TokenPattern { get; }

        /// <summary>
        /// For simple tokens that don't need regex (e.g. "&lt;cms:PageTitle /&gt;").
        /// Ignored if <see cref="TokenPattern"/> is non-null.
        /// </summary>
        string? SimpleToken { get; }

        /// <summary>
        /// Execution order. Lower values run first. Built-in structural tokens (Header, Footer,
        /// Content) use 0-100. Standard tokens use 100-500. Module tokens use 500+.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Render the token. For regex-based tokens, called once per match.
        /// For simple tokens, called once and the return value replaces all occurrences.
        /// </summary>
        Task<string> RenderAsync(Match? match, TokenRenderContext context);
    }
}
