using System;
using Microsoft.UI.Xaml;

namespace ClusterOS
{
    public partial class App : Application
    {
        private Window? m_window;
        public static Window MainWindow => ((App)Current).m_window
        ?? throw new InvalidOperationException("MainWindow não foi inicializada.");

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
