using Microsoft.UI.Xaml.Controls;
using Hub.Domain;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;

namespace ClusterOS.Views
{
    public sealed partial class JournalDateView : Page
    {
        public JournalDateView()
        {
            this.InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Journal journal)
            {
                TitleTextBlock.Text = journal.Created.Date.ToString();
                ContentTextBlock.Text = journal.Content;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
