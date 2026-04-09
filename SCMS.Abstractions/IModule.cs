using Microsoft.Extensions.DependencyInjection;

namespace SCMS.Abstractions
{
    /// <summary>
    /// Entry point for an SCMS module. Implement this interface in a class library,
    /// drop the DLL into /Modules, and the ModuleLoader will discover it at startup.
    /// </summary>
    public interface IModule
    {
        /// <summary>Module display name.</summary>
        string Name { get; }

        /// <summary>Semantic version string.</summary>
        string Version { get; }

        /// <summary>Short description shown in admin.</summary>
        string Description { get; }

        /// <summary>
        /// Register services into the DI container. Called once at startup.
        /// </summary>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Return token handlers this module provides. Called once at startup.
        /// Return empty if the module doesn't add any CMS tokens.
        /// </summary>
        IEnumerable<ITokenHandler> GetTokenHandlers();

        /// <summary>
        /// Return admin menu entries for this module. Called once at startup.
        /// Return empty if the module has no admin UI.
        /// </summary>
        IEnumerable<ModuleAdminMenuItem> GetAdminMenuItems();

        /// <summary>
        /// Return a database setup handler if this module needs its own tables.
        /// Return null if the module has no database requirements.
        /// </summary>
        IModuleDbSetup? GetDbSetup() => null;
    }

    /// <summary>
    /// Represents a menu entry added to the admin panel by a module.
    /// </summary>
    public class ModuleAdminMenuItem
    {
        public string Title { get; }
        public string Url { get; }
        public string IconClass { get; }

        public ModuleAdminMenuItem(string title, string url, string iconClass = "fas fa-puzzle-piece")
        {
            Title = title;
            Url = url;
            IconClass = iconClass;
        }
    }
}
