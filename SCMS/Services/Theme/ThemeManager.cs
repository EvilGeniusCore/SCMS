using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SCMS.Data;

namespace SCMS.Services.Theme
{
    public interface IThemeManager
    {
        Task<string> GetCurrentThemeAsync(ApplicationDbContext db);
        void InvalidateCache();
    }

    public class ThemeManager : IThemeManager
    {
        private const string CacheKey = "CurrentThemeName";
        private readonly IMemoryCache _cache;

        public ThemeManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<string> GetCurrentThemeAsync(ApplicationDbContext db)
        {
            if (_cache.TryGetValue(CacheKey, out string? cached) && cached != null)
                return cached;

            var settings = await db.SiteSettings
                .Include(s => s.Theme)
                .FirstOrDefaultAsync();

            var themeName = settings?.Theme?.Name ?? "Default";

            _cache.Set(CacheKey, themeName, TimeSpan.FromMinutes(30));
            return themeName;
        }

        public void InvalidateCache()
        {
            _cache.Remove(CacheKey);
        }
    }
}
