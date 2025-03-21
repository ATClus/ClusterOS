using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using WinRT.Interop;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinGraphics = Windows.Graphics;
using ClusterOS.Helpers;
using ClusterOS.Managers;
using ClusterOS.InteropPro;
using Windows.Storage;
using ClusterOS.Models;
using ClusterOS.Views;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace ClusterOS.AppsWindows
{
    public sealed partial class DockWindow : Window
    {
        private List<ShortcutData> shortcuts = new List<ShortcutData>();
        public static DockWindow Current { get; private set; }
        private AppWindow appWindow;
        private ProcessManager processManager;
        private const double PanelPadding = 8.0;
        private DockOrientationModel dockOrientation = DockOrientationModel.HorizontalBottom;
        private double iconSize = 56;
        private const double ShortcutMargin = 12;
        private const int HorizontalMargin = 100;
        private const int VerticalMargin = 100;
        private const int TOGGLE_VISIBILITY_ID = 1;
        private const int TOGGLE_ZORDER_ID = 2;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint VK_D = 0x44;
        private const uint VK_Z = 0x5A;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        public DockWindow()
        {
            this.InitializeComponent();
            Current = this;
            this.Closed += DockWindow_Closed;
            this.ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new DesktopAcrylicBackdrop();
            HideWindowFromTaskbar();

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(windowId);

            LoadConfigFromSettings();

            if (dockOrientation == DockOrientationModel.VerticalLeft ||
                dockOrientation == DockOrientationModel.VerticalRight)
            {
                ShortcutPanel.Orientation = Orientation.Vertical;
            }
            else
            {
                ShortcutPanel.Orientation = Orientation.Horizontal;
            }

            UpdateDockSize();
            UpdateDockPosition();

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsMinimizable = false;
                presenter.IsMaximizable = false;
                presenter.SetBorderAndTitleBar(true, false);
                presenter.IsResizable = false;
                presenter.IsAlwaysOnTop = true;
            }

            processManager = new ProcessManager(this, appWindow);
            SetupGlobalHotkey();
            LoadSavedShortcuts();
        }

        private void HideWindowFromTaskbar()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            int exStyle = NativeMethods.GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle &= ~WS_EX_APPWINDOW;
            exStyle |= WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        private void LoadConfigFromSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.TryGetValue("DockPosition", out var pos))
            {
                int index = Convert.ToInt32(pos);
                switch (index)
                {
                    case 0:
                        dockOrientation = DockOrientationModel.HorizontalBottom;
                        break;
                    case 1:
                        dockOrientation = DockOrientationModel.HorizontalTop;
                        break;
                    case 2:
                        dockOrientation = DockOrientationModel.VerticalLeft;
                        break;
                    case 3:
                        dockOrientation = DockOrientationModel.VerticalRight;
                        break;
                    default:
                        dockOrientation = DockOrientationModel.HorizontalBottom;
                        break;
                }
            }

            if (localSettings.Values.TryGetValue("IconSize", out var size))
            {
                iconSize = Convert.ToDouble(size);
            }
        }

        private void DockWindow_Closed(object sender, WindowEventArgs e)
        {
            Current = null;
        }

        private void SetupGlobalHotkey()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            NativeMethods.RegisterHotKey(hwnd, TOGGLE_VISIBILITY_ID, MOD_CONTROL | MOD_ALT, VK_D);
            NativeMethods.RegisterHotKey(hwnd, TOGGLE_ZORDER_ID, MOD_CONTROL | MOD_ALT, VK_Z);

            var windowHandle = HwndSubclass.FromHwnd(hwnd);
            windowHandle.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                switch (hotkeyId)
                {
                    case TOGGLE_VISIBILITY_ID:
                        processManager.DockToggleVisibility();
                        handled = true;
                        break;
                    case TOGGLE_ZORDER_ID:
                        processManager.DockToggleZOrder();
                        handled = true;
                        break;
                }
            }
            return IntPtr.Zero;
        }

        private async void LoadSavedShortcuts()
        {
            shortcuts = await ShortcutManager.LoadShortcutsAsync();
            foreach (var shortcut in shortcuts)
            {
                CreateShortcutButton(shortcut.Path);
            }
            UpdateDockSize();
            UpdateDockPosition();
        }

        private void AnimateScale(ScaleTransform transform, double target)
        {
            var storyboard = new Storyboard();
            var animX = new DoubleAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            Storyboard.SetTarget(animX, transform);
            Storyboard.SetTargetProperty(animX, "ScaleX");
            storyboard.Children.Add(animX);

            var animY = new DoubleAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            Storyboard.SetTarget(animY, transform);
            Storyboard.SetTargetProperty(animY, "ScaleY");
            storyboard.Children.Add(animY);

            storyboard.Begin();
        }

        private void CreateShortcutButton(string path)
        {
            var icon = IconHelper.GetIconUsingShellImageList(path);

            double borderWidth = iconSize, borderHeight = iconSize;
            if (dockOrientation == DockOrientationModel.VerticalLeft ||
                dockOrientation == DockOrientationModel.VerticalRight)
            {
                borderWidth = iconSize;
                borderHeight = iconSize;
            }

            var border = new Border
            {
                Background = new SolidColorBrush(Colors.Transparent),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Width = borderWidth,
                Height = borderHeight,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 },
                Tag = path
            };

            var indicator = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(Colors.Transparent),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var progressRing = new ProgressRing
            {
                IsActive = false,
                Width = 20,
                Height = 20,
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var grid = new Grid();
            grid.Children.Add(new Viewbox
            {
                Stretch = Stretch.Uniform,
                Child = new Image
                {
                    Source = icon,
                    Stretch = Stretch.Uniform,
                }
            });
            grid.Children.Add(progressRing);
            grid.Children.Add(indicator);
            border.Child = grid;

            var scaleTransform = border.RenderTransform as ScaleTransform;
            border.PointerEntered += (s, e) => AnimateScale(scaleTransform, 1.25);
            border.PointerExited += (s, e) => AnimateScale(scaleTransform, 1.0);

            border.Tapped += (s, e) =>
            {
                if (border.Tag is string shortcutPath)
                    processManager.DockHandleApplicationLaunch(border, shortcutPath, progressRing, indicator);
            };

            border.RightTapped += (s, e) =>
            {
                if (border.Tag is string shortcutPath)
                    ShowContextMenu(border, shortcutPath, e);
            };

            ShortcutPanel.Children.Add(border);
            UpdateDockSize();
            UpdateDockPosition();
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
                UpdateDockSize();
                UpdateDockPosition();
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }

        private async void RemoveShortcut(string path)
        {
            shortcuts.RemoveAll(s => s.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
            for (int i = 0; i < ShortcutPanel.Children.Count; i++)
            {
                if (ShortcutPanel.Children[i] is Border border &&
                    border.Tag is string btnPath &&
                    btnPath.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    ShortcutPanel.Children.RemoveAt(i);
                    break;
                }
            }
            await ShortcutManager.SaveShortcutsAsync(shortcuts);
            UpdateDockSize();
            UpdateDockPosition();
        }

        private void ShowContextMenu(Border border, string path, RightTappedRoutedEventArgs e)
        {
            MenuFlyout contextMenu = new MenuFlyout();

            MenuFlyoutItem openNewInstance = new MenuFlyoutItem
            {
                Text = "Open new window",
                Icon = new SymbolIcon(Symbol.Add)
            };
            openNewInstance.Click += (s, args) => processManager.OpenNewInstance(path);

            MenuFlyoutItem closeItem = new MenuFlyoutItem
            {
                Text = "Close application",
                Icon = new SymbolIcon(Symbol.Cancel)
            };
            closeItem.Click += (s, args) => processManager.CloseApplication(processManager.ExtractProcessName(path));

            MenuFlyoutItem minimizeRestoreItem = new MenuFlyoutItem
            {
                Text = "Minimize/Restore",
                Icon = new SymbolIcon(Symbol.Remove)
            };
            minimizeRestoreItem.Click += (s, args) => processManager.MinimizeRestoreApplication(processManager.ExtractProcessName(path));

            MenuFlyoutItem toggleVisibilityItem = new MenuFlyoutItem
            {
                Text = "Hide/Show Dock",
                Icon = new SymbolIcon(Symbol.View)
            };
            toggleVisibilityItem.Click += (s, args) => processManager.DockToggleVisibility();

            MenuFlyoutItem toggleZOrderItem = new MenuFlyoutItem
            {
                Text = "Send Behind/Bring Front",
                Icon = new SymbolIcon(Symbol.Sort)
            };
            toggleZOrderItem.Click += (s, args) => processManager.DockToggleZOrder();

            MenuFlyoutItem configItem = new MenuFlyoutItem
            {
                Text = "Settings",
                Icon = new SymbolIcon(Symbol.Setting)
            };
            configItem.Click += (s, args) => NavigateToDockView();

            MenuFlyoutItem removeItem = new MenuFlyoutItem
            {
                Text = "Remove shortcut",
                Icon = new SymbolIcon(Symbol.Delete)
            };
            removeItem.Click += (s, args) => RemoveShortcut(path);

            contextMenu.Items.Add(openNewInstance);
            contextMenu.Items.Add(closeItem);
            contextMenu.Items.Add(minimizeRestoreItem);
            contextMenu.Items.Add(new MenuFlyoutSeparator());
            contextMenu.Items.Add(toggleVisibilityItem);
            contextMenu.Items.Add(toggleZOrderItem);
            contextMenu.Items.Add(new MenuFlyoutSeparator());
            contextMenu.Items.Add(configItem);
            contextMenu.Items.Add(removeItem);
            contextMenu.ShowAt(border, e.GetPosition(border));
        }

        private void NavigateToDockView()
        {
            var dockViewWindow = new Window
            {
                Content = new DockView(),
                Title = "Dock Settings"
            };
            dockViewWindow.Activate();
        }

        private void UpdateDockSize()
        {
            int count = ShortcutPanel.Children.Count;
            var hwnd = WindowNative.GetWindowHandle(this);

            if (count == 0)
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
                return;
            }
            else
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);
            }

            double totalSpacing = (count - 1) * ShortcutPanel.Spacing;
            double totalPadding = 2 * PanelPadding;
            double totalSize = (count * iconSize) + totalSpacing + totalPadding;

            int extraPixels = 17;
            int newWidth, newHeight;

            if (dockOrientation == DockOrientationModel.HorizontalTop ||
                dockOrientation == DockOrientationModel.HorizontalBottom)
            {
                newWidth = (int)Math.Ceiling(totalSize) + extraPixels;

                int desiredHeight = (int)Math.Ceiling(iconSize + totalPadding);
                newHeight = Math.Max(desiredHeight, 80);
            }
            else
            {
                newHeight = (int)Math.Ceiling(totalSize) + extraPixels;

                int desiredWidth = (int)Math.Ceiling(iconSize + totalPadding);
                newWidth = Math.Max(desiredWidth, 80);
            }

            appWindow.Resize(new WinGraphics.SizeInt32(newWidth, newHeight));
        }

        private void UpdateDockPosition()
        {
            var windowId = Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this));

            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            if (displayArea == null)
            {
                return;
            }

            var workArea = displayArea.WorkArea;
            int workX = workArea.X;
            int workY = workArea.Y;
            int workWidth = workArea.Width;
            int workHeight = workArea.Height;

            int currentWidth = appWindow.Size.Width;
            int currentHeight = appWindow.Size.Height;

            int margin = 0;

            int x = 0;
            int y = 0;

            switch (dockOrientation)
            {
                case DockOrientationModel.HorizontalTop:
                    x = workX + (workWidth - currentWidth) / 2;
                    y = workY + margin;
                    break;

                case DockOrientationModel.HorizontalBottom:
                    x = workX + (workWidth - currentWidth) / 2;
                    y = (workY + workHeight) - currentHeight - margin;
                    break;

                case DockOrientationModel.VerticalLeft:
                    x = workX + margin;
                    y = workY + (workHeight - currentHeight) / 2;
                    break;

                case DockOrientationModel.VerticalRight:
                    x = (workX + workWidth) - currentWidth - margin;
                    y = workY + (workHeight - currentHeight) / 2;
                    break;

                default:
                    x = workX + (workWidth - currentWidth) / 2;
                    y = (workY + workHeight) - currentHeight - margin;
                    break;
            }

            appWindow.Move(new WinGraphics.PointInt32(x, y));
        }
    }
}
