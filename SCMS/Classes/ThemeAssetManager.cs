namespace SCMS.Classes
{
    public static class ThemeAssetManager
    {
        public static void EnsureThemeAssets()
        {
            var themesRoot = Path.Combine(Directory.GetCurrentDirectory(), "Themes");
            if (!Directory.Exists(themesRoot)) return;

            var assetFolders = new[] { "css", "js", "images", "fonts" };

            foreach (var themeDir in Directory.GetDirectories(themesRoot))
            {
                var themeName = Path.GetFileName(themeDir);
                var targetBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Themes", themeName);

                foreach (var folder in assetFolders)
                {
                    var srcDir = Path.Combine(themeDir, folder);
                    var tgtDir = Path.Combine(targetBase, folder);

                    if (!Directory.Exists(srcDir)) continue;

                    Directory.CreateDirectory(tgtDir);

                    foreach (var file in Directory.GetFiles(srcDir))
                    {
                        var destFile = Path.Combine(tgtDir, Path.GetFileName(file));
                        if (!File.Exists(destFile) || File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(destFile))
                        {
                            File.Copy(file, destFile, overwrite: true);
                        }
                    }
                }
            }
        }
    }
}
