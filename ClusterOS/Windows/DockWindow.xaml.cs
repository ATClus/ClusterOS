using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinGraphics = Windows.Graphics;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using ClusterOS.Helpers;

namespace ClusterOS.Windows
{
    public sealed partial class DockWindow : Window
    {
        private List<ShortcutData> shortcuts = new List<ShortcutData>();

        public DockWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;

            SystemBackdrop = new DesktopAcrylicBackdrop();

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new WinGraphics.SizeInt32(800, 80));
            appWindow.Move(new WinGraphics.PointInt32(880, 1360));

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsMinimizable = false;
                presenter.IsMaximizable = false;
                presenter.SetBorderAndTitleBar(true, false);
                presenter.IsResizable = false;
                presenter.IsAlwaysOnTop = true;
            }

            LoadSavedShortcuts();
        }

        private async void LoadSavedShortcuts()
        {
            shortcuts = await ShortcutManager.LoadShortcutsAsync();

            foreach (var shortcut in shortcuts)
            {
                CreateShortcutButton(shortcut.Path);
            }
        }

        private void CreateShortcutButton(string path)
        {
            var icon = IconHelper.GetIconUsingShellImageList(path);

            var button = new Button
            {
                Margin = new Thickness(6),
                Tag = path,
                Background = new SolidColorBrush(Colors.Transparent),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                Padding = new Thickness(0),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 },

                Content = new Grid
                {
                    Width = 56,
                    Height = 56,
                    Children =
                    {
                        new Viewbox
                        {
                            Stretch = Stretch.Uniform,
                            Child = new Image
                            {
                                Source = icon,
                                Stretch = Stretch.Uniform
                            }
                        }
                    }
                }
            };

            var scaleTransform = button.RenderTransform as ScaleTransform;

            button.PointerEntered += (s, e) =>
            {
                scaleTransform.ScaleX = 1.3;
                scaleTransform.ScaleY = 1.3;
            };

            button.PointerExited += (s, e) =>
            {
                scaleTransform.ScaleX = 1.0;
                scaleTransform.ScaleY = 1.0;
            };

            button.Click += ShortcutButton_Click;
            button.RightTapped += ShortcutButton_RightTapped;

            ShortcutPanel.Children.Add(button);
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                foreach (var item in items)
                {
                    if (!shortcuts.Exists(s => s.Path.Equals(item.Path, StringComparison.OrdinalIgnoreCase)))
                    {
                        shortcuts.Add(new ShortcutData { Path = item.Path });

                        CreateShortcutButton(item.Path);

                        await ShortcutManager.SaveShortcutsAsync(shortcuts);
                    }
                }
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }

        private void ShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
        }

        private async void RemoveShortcut(string path)
        {
            shortcuts.RemoveAll(s => s.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

            for (int i = 0; i < ShortcutPanel.Children.Count; i++)
            {
                if (ShortcutPanel.Children[i] is Button btn &&
                    btn.Tag is string btnPath &&
                    btnPath.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    ShortcutPanel.Children.RemoveAt(i);
                    break;
                }
            }

            await ShortcutManager.SaveShortcutsAsync(shortcuts);
        }

        private void ShortcutButton_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                MenuFlyout contextMenu = new MenuFlyout();

                MenuFlyoutItem removeItem = new MenuFlyoutItem
                {
                    Text = "Remover atalho",
                    Icon = new SymbolIcon(Symbol.Delete)
                };

                removeItem.Click += (s, args) =>
                {
                    if (btn.Tag is string path)
                    {
                        RemoveShortcut(path);
                    }
                };

                contextMenu.Items.Add(removeItem);

                contextMenu.ShowAt(btn, e.GetPosition(btn));
            }
        }
    }
}