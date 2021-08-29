using System.Collections.Generic;

namespace WallpaperSorter.Settings
{
    public class Settings
    {
        public string UnsortedWallpaperDirectory { get; set; }
        public string WallpaperDirectory { get; set; }
        public string MassWallpaperDirectory { get; set; }
        public List<string> Genres { get; set; }
    }
}
