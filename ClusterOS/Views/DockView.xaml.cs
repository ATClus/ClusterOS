using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.AppsWindows;
using ClusterOS.Helpers;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ClusterOS.Views
{
    public class ShortcutItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string IconPath { get; set; }
    }

    public sealed partial class DockView : Page
    {
        private ObservableCollection<ShortcutItem> shortcutItems;
        private ApplicationDataContainer localSettings;

        public DockView()
        {
            this.InitializeComponent();
            this.Loaded += DockView_Loaded;
        }

        private void DockView_Loaded(object sender, RoutedEventArgs e)
        {
            localSettings = ApplicationData.Current.LocalSettings;
            shortcutItems = new ObservableCollection<ShortcutItem>();
            ShortcutsListView.ItemsSource = shortcutItems;

            LoadShortcuts();
            LoadSettings();
        }

        private void OpenClusterDock(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            var clusterDockerWindow = new DockWindow();
            clusterDockerWindow.Activate();
        }

        private async void LoadShortcuts()
        {
            shortcutItems.Clear();
            var shortcuts = await ShortcutManager.LoadShortcutsAsync();

            foreach (var shortcut in shortcuts)
            {
                shortcutItems.Add(new ShortcutItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = System.IO.Path.GetFileNameWithoutExtension(shortcut.Path),
                    Path = shortcut.Path,
                    IconPath = shortcut.IconPath ?? "/Assets/DefaultIcon.png"
                });
            }
        }

        private async void AddShortcut_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".exe");
            filePicker.FileTypeFilter.Add(".lnk");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                var shortcutItem = new ShortcutItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = System.IO.Path.GetFileNameWithoutExtension(file.Name),
                    Path = file.Path,
                    IconPath = await ShortcutManager.ExtractIconFromFileAsync(file.Path) ?? "/Assets/DefaultIcon.png"
                };

                shortcutItems.Add(shortcutItem);
                await SaveShortcutsToManagerAsync();
            }
        }

        private void RemoveShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string shortcutId)
            {
                var itemToRemove = shortcutItems.FirstOrDefault(s => s.Id == shortcutId);
                if (itemToRemove != null)
                {
                    shortcutItems.Remove(itemToRemove);
                    SaveShortcutOrder_Click(null, null);
                }
            }
        }

        private async void SaveShortcutOrder_Click(object sender, RoutedEventArgs e)
        {
            await SaveShortcutsToManagerAsync();
        }

        private async Task SaveShortcutsToManagerAsync()
        {
            var shortcutsToSave = shortcutItems.Select(s => new ShortcutData
            {
                Path = s.Path,
                Name = s.Name,
                IconPath = s.IconPath,
                Order = shortcutItems.IndexOf(s)
            }).ToList();

            await ShortcutManager.SaveShortcutsAsync(shortcutsToSave);
        }

        private void LoadSettings()
        {
            if (localSettings.Values.TryGetValue("DockPosition", out var position))
            {
                DockPositionComboBox.SelectedIndex = Convert.ToInt32(position);
            }

            if (localSettings.Values.TryGetValue("IconSize", out var iconSize))
            {
                IconSizeSlider.Value = Convert.ToDouble(iconSize);
            }

            if (localSettings.Values.TryGetValue("DockStyle", out var style))
            {
                DockStyleComboBox.SelectedIndex = Convert.ToInt32(style);
            }

            if (localSettings.Values.TryGetValue("Transparency", out var transparency))
            {
                TransparencySlider.Value = Convert.ToDouble(transparency);
            }

            if (localSettings.Values.TryGetValue("AutoHide", out var autoHide))
            {
                AutoHideToggle.IsOn = Convert.ToBoolean(autoHide);
            }

            if (localSettings.Values.TryGetValue("Animations", out var animations))
            {
                AnimationsToggle.IsOn = Convert.ToBoolean(animations);
            }

            if (localSettings.Values.TryGetValue("Magnification", out var magnification))
            {
                MagnificationToggle.IsOn = Convert.ToBoolean(magnification);
            }
        }

        private void SaveSettings()
        {
            localSettings.Values["DockPosition"] = DockPositionComboBox.SelectedIndex;
            localSettings.Values["IconSize"] = IconSizeSlider.Value;
            localSettings.Values["DockStyle"] = DockStyleComboBox.SelectedIndex;
            localSettings.Values["Transparency"] = TransparencySlider.Value;
            localSettings.Values["AutoHide"] = AutoHideToggle.IsOn;
            localSettings.Values["Animations"] = AnimationsToggle.IsOn;
            localSettings.Values["Magnification"] = MagnificationToggle.IsOn;
        }

        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            SaveShortcutOrder_Click(null, null);

            if (DockWindow.Current != null)
            {
                DockWindow.Current.Close();
            }

            var newDock = new DockWindow();
            newDock.Activate();
        }

        private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            DockPositionComboBox.SelectedIndex = 0;
            IconSizeSlider.Value = 48;
            DockStyleComboBox.SelectedIndex = 0;
            TransparencySlider.Value = 0.2;
            AutoHideToggle.IsOn = false;
            AnimationsToggle.IsOn = true;
            MagnificationToggle.IsOn = true;

            SaveSettings();
        }

        private void StopDock_Click(object sender, RoutedEventArgs e)
        {
            if (DockWindow.Current != null)
            {
                DockWindow.Current.Close();
            }
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string id)
            {
                var item = shortcutItems.FirstOrDefault(s => s.Id == id);
                if (item != null)
                {
                    int index = shortcutItems.IndexOf(item);
                    if (index > 0)
                    {
                        shortcutItems.Move(index, index - 1);
                        SaveShortcutOrder_Click(null, null);
                    }
                }
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string id)
            {
                var item = shortcutItems.FirstOrDefault(s => s.Id == id);
                if (item != null)
                {
                    int index = shortcutItems.IndexOf(item);
                    if (index < shortcutItems.Count - 1)
                    {
                        shortcutItems.Move(index, index + 1);
                        SaveShortcutOrder_Click(null, null);
                    }
                }
            }
        }
    }
}