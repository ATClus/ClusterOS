using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using WinRT.Interop;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinGraphics = Windows.Graphics;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using ClusterOS.Helpers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Shapes;
using System.Linq;
using ClusterOS.Views;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace ClusterOS.AppsWindows
{
    internal static class NativeMethods
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int LWA_COLORKEY = 0x1;
        public const int LWA_ALPHA = 0x2;
        public const int SW_RESTORE = 9;
        public const int SW_MINIMIZE = 6;
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static IntPtr GetMainWindowHandle(Process process)
        {
            IntPtr hWnd = process.MainWindowHandle;
            if (hWnd != IntPtr.Zero)
                return hWnd;

            foreach (ProcessThread thread in process.Threads)
            {
                hWnd = GetWindowHandleFromThread(thread.Id);
                if (hWnd != IntPtr.Zero)
                    return hWnd;
            }
            return IntPtr.Zero;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        private static IntPtr GetWindowHandleFromThread(int threadId)
        {
            IntPtr hWnd = IntPtr.Zero;
            EnumThreadWindows(threadId, (hWndTemp, lParam) =>
            {
                hWnd = hWndTemp;
                return false;
            }, IntPtr.Zero);
            return hWnd;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
    }

    public static class HwndSubclass
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProc")]
        private static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static readonly int GWLP_WNDPROC = -4;
        private static readonly Dictionary<IntPtr, List<HwndSourceHook>> _hooks = new Dictionary<IntPtr, List<HwndSourceHook>>();
        private static readonly Dictionary<IntPtr, IntPtr> _origWndProcs = new Dictionary<IntPtr, IntPtr>();

        public delegate IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);

        public static HwndSource FromHwnd(IntPtr hwnd)
        {
            return new HwndSource(hwnd);
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (_hooks.TryGetValue(hwnd, out var hooks))
            {
                bool handled = false;
                foreach (var hook in hooks)
                {
                    hook(hwnd, msg, wParam, lParam, ref handled);
                    if (handled)
                        return IntPtr.Zero;
                }
            }

            return _origWndProcs.TryGetValue(hwnd, out var origWndProc)
                ? CallWindowProcW(origWndProc, hwnd, msg, wParam, lParam)
                : IntPtr.Zero;
        }

        private static readonly WndProcDelegate s_wndProcDelegate = WndProc;
        private static readonly IntPtr s_wndProcFunctionPtr = Marshal.GetFunctionPointerForDelegate(s_wndProcDelegate);

        private delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        public class HwndSource
        {
            private readonly IntPtr _hwnd;

            public HwndSource(IntPtr hwnd)
            {
                _hwnd = hwnd;
            }

            public void AddHook(HwndSourceHook hook)
            {
                if (!_hooks.TryGetValue(_hwnd, out var hooks))
                {
                    hooks = new List<HwndSourceHook>();
                    _hooks[_hwnd] = hooks;

                    IntPtr origWndProc;
                    if (IntPtr.Size == 8)
                        origWndProc = SetWindowLongPtrW(_hwnd, GWLP_WNDPROC, s_wndProcFunctionPtr);
                    else
                        origWndProc = SetWindowLongW(_hwnd, GWLP_WNDPROC, s_wndProcFunctionPtr);

                    _origWndProcs[_hwnd] = origWndProc;
                }

                if (!_hooks[_hwnd].Contains(hook))
                    _hooks[_hwnd].Add(hook);
            }
        }
    }

    public sealed partial class DockWindow : Window
    {
        private List<ShortcutData> shortcuts = new List<ShortcutData>();
        private static Dictionary<string, int> activeProcesses = new Dictionary<string, int>();
        private bool isDockOnTop = true;
        private bool isVisible = true;
        public static DockWindow Current { get; private set; }
        private AppWindow appWindow;

        private const int TOGGLE_VISIBILITY_ID = 1;
        private const int TOGGLE_ZORDER_ID = 2;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint VK_D = 0x44;
        private const uint VK_Z = 0x5A;

        public DockWindow()
        {
            this.InitializeComponent();
            Current = this;
            this.Closed += DockWindow_Closed;
            this.ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new DesktopAcrylicBackdrop();

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(windowId);
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

            SetupGlobalHotkey();
            LoadSavedShortcuts();
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
                        ToggleDockVisibility();
                        handled = true;
                        break;

                    case TOGGLE_ZORDER_ID:
                        ToggleDockZOrder();
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }

        private void ToggleDockVisibility()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            if (isVisible)
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
            }
            else
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);
            }

            isVisible = !isVisible;
        }

        private void ToggleDockZOrder()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            if (isDockOnTop)
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_BOTTOM, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);

                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsAlwaysOnTop = false;
                }
            }
            else
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);

                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsAlwaysOnTop = true;
                }
            }

            isDockOnTop = !isDockOnTop;
        }

        private async void LoadSavedShortcuts()
        {
            shortcuts = await ShortcutManager.LoadShortcutsAsync();
            foreach (var shortcut in shortcuts)
            {
                CreateShortcutButton(shortcut.Path);
            }
        }

        private string ExtractProcessName(string path)
        {
            return !string.IsNullOrEmpty(path) ? System.IO.Path.GetFileNameWithoutExtension(path)
.ToLower() : string.Empty;
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

        private async void HandleApplicationLaunch(Border border, string path, ProgressRing progressRing, Ellipse indicator)
        {
            try
            {
                string extractedProcessName = ExtractProcessName(path);

                Dictionary<string, string> browserProcesses = new Dictionary<string, string>
                {
                    { "chrome", "chrome" },
                    { "msedge", "msedge" },
                    { "firefox", "firefox" },
                    { "opera", "opera" },
                    { "brave", "brave" },
                    { "zen", "zen" }
                };

                string expectedProcessName = browserProcesses
                    .Where(b => extractedProcessName.Contains(b.Key))
                    .Select(b => b.Value)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(expectedProcessName))
                    expectedProcessName = extractedProcessName;

                if (activeProcesses.TryGetValue(extractedProcessName, out int savedPid))
                {
                    try
                    {
                        Process savedProcess = Process.GetProcessById(savedPid);
                        IntPtr savedWindowHandle = NativeMethods.GetMainWindowHandle(savedProcess);
                        if (savedWindowHandle != IntPtr.Zero && NativeMethods.IsWindowVisible(savedWindowHandle))
                        {
                            if (NativeMethods.IsIconic(savedWindowHandle))
                            {
                                NativeMethods.ShowWindow(savedWindowHandle, NativeMethods.SW_RESTORE);
                            }
                            NativeMethods.SetForegroundWindow(savedWindowHandle);
                            DispatcherQueue.TryEnqueue(() => indicator.Fill = new SolidColorBrush(Colors.Green));
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        activeProcesses.Remove(extractedProcessName);
                    }
                }

                Process[] processes = Process.GetProcessesByName(expectedProcessName);
                foreach (var process in processes)
                {
                    try
                    {
                        IntPtr hWnd = NativeMethods.GetMainWindowHandle(process);
                        if (hWnd != IntPtr.Zero && NativeMethods.IsWindowVisible(hWnd))
                        {
                            if (NativeMethods.IsIconic(hWnd))
                            {
                                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                if (process.ProcessName.ToLower() == "firefox")
                                {
                                    await Task.Delay(200);
                                    if (NativeMethods.IsIconic(hWnd))
                                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                }
                            }
                            NativeMethods.SetForegroundWindow(hWnd);
                            activeProcesses[extractedProcessName] = process.Id;
                            DispatcherQueue.TryEnqueue(() => indicator.Fill = new SolidColorBrush(Colors.Green));
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DEBUG] Process error: {process.ProcessName}: {ex.Message}");
                    }
                }

                progressRing.Visibility = Visibility.Visible;
                progressRing.IsActive = true;

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };

                var processStarted = Process.Start(startInfo);
                await Task.Delay(1000);

                processes = Process.GetProcessesByName(expectedProcessName);
                foreach (var process in processes)
                {
                    try
                    {
                        IntPtr hWnd = NativeMethods.GetMainWindowHandle(process);
                        if (hWnd != IntPtr.Zero && NativeMethods.IsWindowVisible(hWnd))
                        {
                            if (NativeMethods.IsIconic(hWnd))
                            {
                                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                if (process.ProcessName.ToLower() == "firefox")
                                {
                                    await Task.Delay(200);
                                    if (NativeMethods.IsIconic(hWnd))
                                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                }
                            }
                            NativeMethods.SetForegroundWindow(hWnd);
                            activeProcesses[extractedProcessName] = process.Id;
                            DispatcherQueue.TryEnqueue(() => indicator.Fill = new SolidColorBrush(Colors.Green));
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DEBUG] Process error: {process.ProcessName}: {ex.Message}");
                    }
                }

                Task.Run(() =>
                {
                    processStarted.WaitForExit();
                    activeProcesses.Remove(extractedProcessName);
                    DispatcherQueue.TryEnqueue(() => indicator.Fill = new SolidColorBrush(Colors.Transparent));
                });

                progressRing.Visibility = Visibility.Collapsed;
                progressRing.IsActive = false;
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    indicator.Fill = new SolidColorBrush(Colors.Red);
                    await Task.Delay(2000);
                    indicator.Fill = new SolidColorBrush(Colors.Transparent);
                });
            }
        }

        private void CreateShortcutButton(string path)
        {
            var icon = IconHelper.GetIconUsingShellImageList(path);

            var border = new Border
            {
                Margin = new Thickness(6),
                Background = new SolidColorBrush(Colors.Transparent),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Width = 56,
                Height = 56,
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
                    HandleApplicationLaunch(border, shortcutPath, progressRing, indicator);
            };

            border.RightTapped += (s, e) =>
            {
                if (border.Tag is string shortcutPath)
                    ShowContextMenu(border, shortcutPath, e);
            };

            ShortcutPanel.Children.Add(border);
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
        }

        private void ShowContextMenu(Border border, string path, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            MenuFlyout contextMenu = new MenuFlyout();

            MenuFlyoutItem openNewInstance = new MenuFlyoutItem
            {
                Text = "Open new window",
                Icon = new SymbolIcon(Symbol.Add)
            };
            openNewInstance.Click += (s, args) => OpenNewInstance(path);

            MenuFlyoutItem closeItem = new MenuFlyoutItem
            {
                Text = "Close application",
                Icon = new SymbolIcon(Symbol.Cancel)
            };
            closeItem.Click += (s, args) => CloseApplication(ExtractProcessName(path));

            MenuFlyoutItem minimizeRestoreItem = new MenuFlyoutItem
            {
                Text = "Minimize/Restore",
                Icon = new SymbolIcon(Symbol.Remove)
            };
            minimizeRestoreItem.Click += (s, args) => MinimizeRestoreApplication(ExtractProcessName(path));

            MenuFlyoutItem toggleVisibilityItem = new MenuFlyoutItem
            {
                Text = "Hide/Show Dock",
                Icon = new SymbolIcon(Symbol.View)
            };
            toggleVisibilityItem.Click += (s, args) => ToggleDockVisibility();

            MenuFlyoutItem toggleZOrderItem = new MenuFlyoutItem
            {
                Text = "Send Behind/Bring Front",
                Icon = new SymbolIcon(Symbol.Sort)
            };
            toggleZOrderItem.Click += (s, args) => ToggleDockZOrder();

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

        private void OpenNewInstance(string path)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] Error opening new instance: {ex.Message}");
            }
        }

        private void CloseApplication(string extractedProcessName)
        {
            if (activeProcesses.TryGetValue(extractedProcessName, out int pid))
            {
                try
                {
                    Process process = Process.GetProcessById(pid);
                    if (!process.CloseMainWindow())
                        process.Kill();
                    activeProcesses.Remove(extractedProcessName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] Error closing application: {ex.Message}");
                }
            }
        }

        private void MinimizeRestoreApplication(string extractedProcessName)
        {
            if (activeProcesses.TryGetValue(extractedProcessName, out int pid))
            {
                try
                {
                    Process process = Process.GetProcessById(pid);
                    IntPtr hWnd = NativeMethods.GetMainWindowHandle(process);
                    if (hWnd != IntPtr.Zero)
                    {
                        if (NativeMethods.IsIconic(hWnd))
                            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                        else
                            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_MINIMIZE);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] Error minimizing/restoring application: {ex.Message}");
                }
            }
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
    }
}