using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.Views;
using Microsoft.UI.Xaml.Media.Animation;
using TrayServiceLib;
using System;
using System.Windows.Forms;

namespace ClusterOS
{
    public sealed partial class MainWindow : Window
    {
        private TrayService trayService;
        private bool hasShownTrayNotification = false;

        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;

            // Acrylic Window
            SystemBackdrop = new DesktopAcrylicBackdrop();

            Root.Navigate(typeof(HomeView));
            Nav.SelectedItem = Nav.MenuItems[0];

            // Tray Service
            trayService = new TrayService();
            trayService.RestoreRequested += OnRestoreRequested;
            trayService.ExitRequested += OnExitRequested;

            // Hide App to Tray
            this.Closed += Window_Closing;
        }

        private void OnRestoreRequested(object sender, EventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    presenter.Minimize(false);
                }

                this.Activate();
            });
        }

        private void Window_Closing(object sender, WindowEventArgs e)
        {
            e.Handled = true;
            this.AppWindow.Hide();

            if (!hasShownTrayNotification)
            {
                trayService.ShowBalloonTip("ClusterOS", "The application continues running in the tray.", ToolTipIcon.Info);
                hasShownTrayNotification = true;
            }
        }

        private void OnExitRequested(object sender, EventArgs e)
        {
            trayService.Dispose();
            Environment.Exit(0);
        }

        private void Navigation(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                Root.Navigate(typeof(SettingsView), null, new EntranceNavigationTransitionInfo());
            }
            else
            {
                var selectedItem = args.SelectedItem as NavigationViewItem;
                switch (selectedItem.Tag)
                {
                    case "HomeView":
                        Root.Navigate(typeof(HomeView), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "TrackerView":
                        Root.Navigate(typeof(TrackerView), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "DockView":
                        Root.Navigate(typeof(DockView), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "JournalView":
                        Root.Navigate(typeof(JournalView), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "TasksView":
                        Root.Navigate(typeof(TasksView), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "CMSView":
                        Root.Navigate(typeof(CMSView), null, new EntranceNavigationTransitionInfo());
                        break;

                    default:
                        Root.Navigate(typeof(HomeView), null, new EntranceNavigationTransitionInfo());
                        break;
                }
            }
        }
    }
}
