using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using ClusterOS.Windows;

namespace ClusterOS
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;

            // Acrylic Window
            SystemBackdrop = new DesktopAcrylicBackdrop();
        }

        public void OpenClusterDock(object sender, RoutedEventArgs e)
        {
            var clusterDockerWindow = new ClusterDock();
            clusterDockerWindow.Activate();
        }
    }
}
