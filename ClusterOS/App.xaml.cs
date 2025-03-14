using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.UI.Xaml;
using ClusterOS.Services;
using System.Threading.Tasks;

namespace ClusterOS
{
    public partial class App : Application
    {
        private Window? m_window;
        private IHost _webHost;
        public static Window MainWindow => ((App)Current).m_window
            ?? throw new InvalidOperationException("MainWindow has not been initialized.");

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();

            m_window.DispatcherQueue.TryEnqueue(async () =>
            {
                _webHost = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<ApiHubService>()
                                  .UseUrls("https://localhost:7131");
                    })
                    .Build();

                await _webHost.StartAsync();
                await ShowNotificationAsync("Hub Started", "The tracker will automatically start");
            });
        }

        private async Task ShowNotificationAsync(string title, string message)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(message, title);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(dialog, hwnd);
            await dialog.ShowAsync();
        }

        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            if (_webHost != null)
            {
                await _webHost.StopAsync(TimeSpan.FromSeconds(5));
                _webHost.Dispose();
            }
        }
    }
}
