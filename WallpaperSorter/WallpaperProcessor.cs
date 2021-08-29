using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WallpaperSorter
{
    public class WallpaperProcessor
    {
        private Settings.Settings settings;

        public WallpaperProcessor(Settings.Settings settings)
        {
            this.settings = settings;
        }

        public void Process(Dictionary<string, (string genre, bool putWallpaperInWallpaperFolder)> wallpapersToSort)
        {
            var genreWallpaperCounts = GetGenreWallpaperCounts();

            foreach (var wallpaperAndCategoryAndPutInWallpaperFolder in wallpapersToSort)
            {
                var wallpaperFileInfo = new FileInfo(wallpaperAndCategoryAndPutInWallpaperFolder.Key);
                var genreAndPutInWallpaperFolder = wallpaperAndCategoryAndPutInWallpaperFolder.Value;

                // Skip of genre has not been chosen
                if (genreAndPutInWallpaperFolder.genre == null) continue;

                var currentCategoryCount = genreWallpaperCounts[genreAndPutInWallpaperFolder.genre];
 
                var newFileName = $"{genreAndPutInWallpaperFolder.genre} ({currentCategoryCount}){wallpaperFileInfo.Extension}";
                genreWallpaperCounts[genreAndPutInWallpaperFolder.genre] += 1;

                MoveWallpaperToFolders(wallpaperFileInfo, genreAndPutInWallpaperFolder.genre, newFileName, genreAndPutInWallpaperFolder.putWallpaperInWallpaperFolder);
            }
        }

        private Dictionary<string, int> GetGenreWallpaperCounts()
        {
            var dict = new Dictionary<string, int>();

            foreach (var genre in settings.Genres)
            {
                var dir = new DirectoryInfo(genre);

                var fileInfos = dir.GetFiles();

                dict[dir.Name] = fileInfos.Count();
            }

            return dict;
        }

        private void MoveWallpaperToFolders(FileInfo currentWallpaper, string genre, string newFileName, bool putWallpaperInWallpaperFolder)
        {
            if (putWallpaperInWallpaperFolder)
                currentWallpaper.CopyTo(Path.Combine(settings.WallpaperDirectory, newFileName));

            var fullGenrePath = settings.Genres.FirstOrDefault(x => x.EndsWith(genre));

            if (string.IsNullOrWhiteSpace(fullGenrePath)) return;

            currentWallpaper.MoveTo(Path.Combine(fullGenrePath, newFileName));
        }
    }
}
