using SCMS.Data;

namespace SCMS.Interfaces
{
    public interface IThemeEngine
    {
        Task<string> RenderAsync(PageContent page, ApplicationDbContext db);
    }
}
