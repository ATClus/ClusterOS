using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.Windows;

namespace ClusterOS.Views
{
    public sealed partial class DockView : Page
    {
        public DockView()
        {
            this.InitializeComponent();
        }

        private void OpenClusterDock(object sender, RoutedEventArgs e)
        {
            var clusterDockerWindow = new DockWindow();
            clusterDockerWindow.Activate();
        }
    }
}
