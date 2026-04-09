using System.Reflection;
using System.Runtime.Loader;
using SCMS.Abstractions;

namespace SCMS.Services
{
    /// <summary>
    /// Discovers and loads SCMS modules from the /Modules folder at startup.
    /// Each module is a .NET class library DLL containing an IModule implementation.
    /// </summary>
    public static class ModuleLoader
    {
        private static readonly List<IModule> _loadedModules = new();

        /// <summary>
        /// All modules discovered and loaded during startup.
        /// </summary>
        public static IReadOnlyList<IModule> LoadedModules => _loadedModules.AsReadOnly();

        /// <summary>
        /// Scans the /Modules folder, loads assemblies, discovers IModule implementations,
        /// calls ConfigureServices, and registers token handlers into DI.
        /// Call this BEFORE builder.Build().
        /// </summary>
        public static void DiscoverAndRegister(IServiceCollection services, ILogger logger)
        {
            var modulesPath = Path.Combine(Directory.GetCurrentDirectory(), "Modules");

            if (!Directory.Exists(modulesPath))
            {
                logger.LogInformation("No /Modules folder found — skipping module discovery");
                return;
            }

            var dlls = Directory.GetFiles(modulesPath, "*.dll");
            if (dlls.Length == 0)
            {
                logger.LogInformation("Modules folder is empty — no modules to load");
                return;
            }

            foreach (var dll in dlls)
            {
                try
                {
                    var assembly = LoadModuleAssembly(dll);
                    var moduleTypes = assembly.GetTypes()
                        .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                    foreach (var moduleType in moduleTypes)
                    {
                        var module = (IModule)Activator.CreateInstance(moduleType)!;
                        _loadedModules.Add(module);

                        logger.LogInformation("Loading module: {ModuleName} v{Version} — {Description}",
                            module.Name, module.Version, module.Description);

                        // Let the module register its own services
                        module.ConfigureServices(services);

                        // Register the module's token handlers
                        foreach (var handler in module.GetTokenHandlers())
                        {
                            services.AddSingleton<ITokenHandler>(handler);
                            logger.LogInformation("  Registered token handler: {HandlerName} (priority {Priority})",
                                handler.Name, handler.Priority);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load module from {DllPath}", dll);
                }
            }

            logger.LogInformation("Module discovery complete — {Count} module(s) loaded", _loadedModules.Count);
        }

        /// <summary>
        /// Run database setup for all loaded modules that provide one.
        /// Call this AFTER core migrations have run.
        /// </summary>
        public static async Task RunModuleDbSetupAsync(IServiceProvider services, ILogger logger)
        {
            foreach (var module in _loadedModules)
            {
                var dbSetup = module.GetDbSetup();
                if (dbSetup == null) continue;

                try
                {
                    logger.LogInformation("Running database setup for module: {ModuleName}", module.Name);
                    await dbSetup.SetupAsync(services);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database setup failed for module: {ModuleName}", module.Name);
                }
            }
        }

        private static Assembly LoadModuleAssembly(string path)
        {
            // Use a separate AssemblyLoadContext so module dependencies don't conflict with the host
            var loadContext = new ModuleLoadContext(path);
            return loadContext.LoadFromAssemblyPath(Path.GetFullPath(path));
        }
    }

    /// <summary>
    /// Isolates module assemblies in their own load context.
    /// Falls back to the default context for shared framework types.
    /// </summary>
    internal class ModuleLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public ModuleLoadContext(string pluginPath) : base(isCollectible: false)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Try to resolve from the module's own deps.json first
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            if (path != null)
                return LoadFromAssemblyPath(path);

            // Fall back to the default context (shared framework, SCMS.Abstractions, etc.)
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return path != null ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
        }
    }
}
