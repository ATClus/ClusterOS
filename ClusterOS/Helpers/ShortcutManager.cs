using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage.Streams;

namespace ClusterOS.Helpers
{
    public class ShortcutData
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public int Order { get; set; }
    }

    public static class ShortcutManager
    {
        private const string ShortcutsFileName = "DockShortcuts.json";
        private const string IconsFolder = "DockShortcutIcons";

        public static async Task SaveShortcutsAsync(List<ShortcutData> shortcuts)
        {
            try
            {
                for (int i = 0; i < shortcuts.Count; i++)
                {
                    shortcuts[i].Order = i;
                }

                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.CreateFileAsync(ShortcutsFileName, CreationCollisionOption.ReplaceExisting);
                string jsonContent = JsonSerializer.Serialize(shortcuts, new JsonSerializerOptions { WriteIndented = true });
                await FileIO.WriteTextAsync(file, jsonContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving shortcuts: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<ShortcutData>> LoadShortcutsAsync()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                if (await localFolder.TryGetItemAsync(ShortcutsFileName) is StorageFile file)
                {
                    string jsonContent = await FileIO.ReadTextAsync(file);
                    var shortcuts = JsonSerializer.Deserialize<List<ShortcutData>>(jsonContent);

                    return shortcuts?.OrderBy(s => s.Order).ToList() ?? CreateDefaultShortcuts();
                }

                var defaultShortcuts = CreateDefaultShortcuts();
                await SaveShortcutsAsync(defaultShortcuts);
                return defaultShortcuts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading shortcuts: {ex.Message}");
                return CreateDefaultShortcuts();
            }
        }

        public static List<ShortcutData> CreateDefaultShortcuts()
        {
            return new List<ShortcutData>
            {
                new ShortcutData { Path = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe", Name = "Microsoft Edge", Order = 0 }
            };
        }

        public static async Task<string> ExtractIconFromFileAsync(string filePath)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder iconsFolder = await localFolder.CreateFolderAsync(IconsFolder, CreationCollisionOption.OpenIfExists);

                string iconFileName = GenerateHashFromPath(filePath) + ".png";

                if (await iconsFolder.TryGetItemAsync(iconFileName) != null)
                {
                    return $"ms-appdata:///local/{IconsFolder}/{iconFileName}";
                }

                StorageFile iconFile;

                if (filePath.Contains("edge") || filePath.EndsWith("msedge.exe"))
                {
                    iconFile = await Package.Current.InstalledLocation.GetFileAsync("Assets\\Icons\\logo.png");
                }
                else
                {
                    iconFile = await Package.Current.InstalledLocation.GetFileAsync("Assets\\Icons\\default_app.png");
                }

                StorageFile destinationFile = await iconsFolder.CreateFileAsync(iconFileName, CreationCollisionOption.ReplaceExisting);

                using (var sourceStream = await iconFile.OpenAsync(FileAccessMode.Read))
                using (var destinationStream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAsync(sourceStream, destinationStream);
                }

                return $"ms-appdata:///local/{IconsFolder}/{iconFileName}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao extrair ícone: {ex.Message}");
                return null;
            }
        }

        public static async Task<ShortcutData> AddShortcutFromFilePickerAsync(object window)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".exe");
            filePicker.FileTypeFilter.Add(".lnk");
            filePicker.SuggestedStartLocation = PickerLocationId.Desktop;

            if (window != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            }

            var file = await filePicker.PickSingleFileAsync();
            if (file == null)
                return null;

            var shortcut = new ShortcutData
            {
                Path = file.Path,
                Name = Path.GetFileNameWithoutExtension(file.Name),
                IconPath = await ExtractIconFromFileAsync(file.Path),
                Order = -1
            };

            return shortcut;
        }

        private static string GenerateHashFromPath(string path)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(path);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        public static async Task ReorderShortcutsAsync(ObservableCollection<ShortcutData> items)
        {
            var shortcuts = items.ToList();
            await SaveShortcutsAsync(shortcuts);
        }

        public static async Task DeleteShortcutAsync(ObservableCollection<ShortcutData> items, ShortcutData shortcut)
        {
            items.Remove(shortcut);
            await SaveShortcutsAsync(items.ToList());
        }
    }
}
