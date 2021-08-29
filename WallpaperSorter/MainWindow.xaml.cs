using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WallpaperSorter.Settings;

namespace WallpaperSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Settings.Settings settings;
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private Dictionary<string, (string genre, bool putWallpaperInWallpaperFolder)> unsortedWallpapers = new Dictionary<string, (string genre, bool putWallpaperInWallpaperFolder)>();
        private readonly List<RadioButton> radioButtonsList = new List<RadioButton>();
        private WallpaperProcessor processor;

        private int count = 0;
        private int unsortedFileCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            RegisterShiftKeysListeners();

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            settings = SettingsHelper.LoadSettings();
            processor = new WallpaperProcessor(settings);
            
            LoadUnsortedWallpapers();
            SetGenreRadioButtons();            
            SetFirstUnsortedWallpaper();
        }

        private void RegisterShiftKeysListeners()
        {
            this.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                    ShowShiftKeyDown.Content = "Skip placing wallpaper in wallpaper folder!";
            };

            this.KeyUp += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                    ShowShiftKeyDown.Content = "";
            };
        }

        private void LoadUnsortedWallpapers()
        {
            var files = Directory.GetFiles(settings.UnsortedWallpaperDirectory);
            var test = new FileInfo(files.First()).Extension;
            unsortedWallpapers = files.Where(f =>
                {
                    var fileInfo = new FileInfo(f);
                    return fileInfo.Exists && FileExtensionsHelper.AllowedExtensions.Contains(fileInfo.Extension);
                }).ToDictionary(x => x, y => ((string)null, true));

            unsortedFileCount = unsortedWallpapers.Count;
        }

        private void SetFirstUnsortedWallpaper()
        {
            if (unsortedWallpapers.Any())
                SetWallpaper(unsortedWallpapers.First().Key);
        }

        private void SetWallpaper(string wallpaperLocation)
        {
            var wallpaperFileInfo = new FileInfo(wallpaperLocation);
            var wallpaperUri = new Uri(wallpaperLocation);

            var bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();

            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.UriSource = wallpaperUri;
            bitmapSource.EndInit();

            SetFittingDimensions(bitmapSource);

            var unsortedWallpaperAndPutIntoWallpaperFolder = unsortedWallpapers[wallpaperLocation];

            Counter.Content = $"{count + 1}/{unsortedFileCount}";
            ChosenCategory.Content = unsortedWallpaperAndPutIntoWallpaperFolder.GetContentText();
            WallpaperName.Content = wallpaperFileInfo.Name;
            UnsortedWallpaperView.Source = bitmapSource;
            Dimensions.Content = $"Width: {bitmapSource.PixelWidth}px, Height: {bitmapSource.PixelHeight}px, X-DPI: {bitmapSource.DpiX}, Y-DPI: {bitmapSource.DpiY}";
        }

        private void SetFittingDimensions(BitmapImage bitmapSource)
        {
            var heightMultiplier = (944 / bitmapSource.Height);

            UnsortedWallpaperView.Height = 944;
            UnsortedWallpaperView.Width = bitmapSource.Width * heightMultiplier;


            if (UnsortedWallpaperView.Width > 1678)
            {
                var widthMultiplier = (1678 / bitmapSource.Width);

                UnsortedWallpaperView.Width = 1678;
                UnsortedWallpaperView.Height = bitmapSource.Height * widthMultiplier;
            }
        }

        private void SetGenreRadioButtons()
        {
            foreach(var genre in settings.Genres)
            {
                var name = new DirectoryInfo(genre).Name;

                var radioButton = new RadioButton
                {
                    Content = name,
                    FontSize = 20,
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(0, 0, 0, 5),
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                radioButton.Checked += (object sender, RoutedEventArgs e) => 
                {
                    var rb = sender as RadioButton;

                    var currentWallpaper = unsortedWallpapers.Skip(count).First().Key;

                    SetWallpaperGenre(currentWallpaper, rb.Content as string);

                    rb.IsChecked = false;
                    Next();
                };
                stackPanel.Children.Add(radioButton);
                radioButtonsList.Add(radioButton);
            }
        }

        private void SetWallpaperGenre(string currentWallpaper, string genre)
        {
            var putWallpaperInWallpaperFolder = true;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                putWallpaperInWallpaperFolder = false;

            ChosenCategory.Content = (genre, putWallpaperInWallpaperFolder).GetContentText();;

            unsortedWallpapers[currentWallpaper] = (genre, putWallpaperInWallpaperFolder);
        }
        private void Next()
        {
            if (count < unsortedFileCount - 1) count += 1;
            else return;

            SetWallpaper(unsortedWallpapers.Skip(count).First().Key);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
            => Next();

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (count >= 1) count -= 1;
            else return;

            SetWallpaper(unsortedWallpapers.Skip(count).First().Key);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            UnsortedWallpaperView.Source = null;
            WallpaperName.Content = null;
            Counter.Content = "0/0";
            ProcesssingLabel.Content = $"Processing {unsortedWallpapers.Select(x => x.Value).Where(x => x.genre != null).Count()} wallpapers...";

            SaveButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            PreviousButton.IsEnabled = false;

            foreach (var rb in radioButtonsList)
                rb.IsEnabled = false;

            worker.RunWorkerAsync(unsortedWallpapers);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var wallpapersToSort = e.Argument as Dictionary<string, (string genre, bool putWallpaperInWallpaperFolder)>;

            processor.Process(wallpapersToSort);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProcesssingLabel.Content = "Finished processing all wallpapers";

            Application.Current.Shutdown();
        }
    }
}
