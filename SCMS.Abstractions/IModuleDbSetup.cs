namespace SCMS.Abstractions
{
    /// <summary>
    /// Optional interface a module can implement to set up its database tables.
    /// Called once at startup after core migrations have run.
    /// </summary>
    public interface IModuleDbSetup
    {
        /// <summary>
        /// Ensure the module's database tables exist.
        /// Called with the app's IServiceProvider so the module can resolve
        /// the core ApplicationDbContext or create its own.
        /// </summary>
        Task SetupAsync(IServiceProvider services);
    }
}
