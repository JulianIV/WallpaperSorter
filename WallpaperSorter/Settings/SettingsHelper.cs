using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace WallpaperSorter.Settings
{
    public static class SettingsHelper
    {
        public static Settings LoadSettings()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var settingsFile = $"{currentDirectory}\\Settings\\Settings.json";

            var file = File.ReadAllText(settingsFile);

            var settings = JsonConvert.DeserializeObject<Settings>(file);

            var genres = Directory.GetDirectories(settings.MassWallpaperDirectory, "", SearchOption.AllDirectories).ToList();

            settings.Genres = genres;

            var unsortedWallpaperDirectory = new DirectoryInfo(settings.UnsortedWallpaperDirectory);
            if (!unsortedWallpaperDirectory.Exists)
                throw new Exception($"Invalid {nameof(settings.UnsortedWallpaperDirectory)}: {settings.UnsortedWallpaperDirectory }");

            var wallpaperDirectory = new DirectoryInfo(settings.WallpaperDirectory);
            if (!wallpaperDirectory.Exists)
                throw new Exception($"Invalid {nameof(settings.WallpaperDirectory)}: {settings.WallpaperDirectory }");

            var massWallpaperDirectory = new DirectoryInfo(settings.MassWallpaperDirectory);
            if (!massWallpaperDirectory.Exists)
                throw new Exception($"Invalid {nameof(settings.MassWallpaperDirectory)}: {settings.MassWallpaperDirectory}");

            return settings;
        }
    }
}
