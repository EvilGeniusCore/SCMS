using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SCMS.Abstractions;
using SCMS.Classes;
using SCMS.Data;

namespace SCMS.Services.TokenHandlers
{
    public class MenuTokenHandler : ITokenHandler
    {
        public string Name => "Menu";
        public Regex? TokenPattern => new(
            @"<cms:Menu\s+(?=.*orientation=""(?<orientation>\w+)""\s*)(?=.*group=""(?<group>[^""]+)""\s*).*?\/?>",
            RegexOptions.IgnoreCase);
        public string? SimpleToken => null;
        public int Priority => 300;

        public Task<string> RenderAsync(Match? match, TokenRenderContext context)
        {
            if (match == null) return Task.FromResult("");

            var orientation = match.Groups["orientation"].Value;
            var group = match.Groups["group"].Value;
            var principal = context.HttpContext?.User ?? new ClaimsPrincipal();

            var db = context.Services?.GetService<ApplicationDbContext>();
            if (db == null) return Task.FromResult("");

            return Task.FromResult(MenuBuilder.GenerateMenuHtml(db, group, orientation, principal, context.ThemeName));
        }
    }
}
