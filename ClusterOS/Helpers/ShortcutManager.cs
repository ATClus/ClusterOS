using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClusterOS.Helpers
{
    public class ShortcutData
    {
        public string Path { get; set; }
    }

    public static class ShortcutManager
    {
        private const string ShortcutsFileName = "shortcuts.json";

        public static async Task SaveShortcutsAsync(List<ShortcutData> shortcuts)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.CreateFileAsync(ShortcutsFileName, CreationCollisionOption.ReplaceExisting);

                string jsonContent = JsonSerializer.Serialize(shortcuts, new JsonSerializerOptions { WriteIndented = true });

                await FileIO.WriteTextAsync(file, jsonContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar atalhos: {ex.Message}");
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

                    return JsonSerializer.Deserialize<List<ShortcutData>>(jsonContent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar atalhos: {ex.Message}");
            }

            return new List<ShortcutData>();
        }
    }
}
