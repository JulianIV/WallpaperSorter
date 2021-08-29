namespace WallpaperSorter
{
    public static class TextHelper
    {
        public static string GetContentText(this (string genre, bool putWallpaperInWallpaperFolder) genreAndPutWallpaperInWallpaperFolder)
        {
            var putWallpaperInWallpaperFolderText = genreAndPutWallpaperInWallpaperFolder.putWallpaperInWallpaperFolder ? "" : " (Skipping main wallpaper folder)";
            return $"{genreAndPutWallpaperInWallpaperFolder.genre}{putWallpaperInWallpaperFolderText}";
        }
    }
}
