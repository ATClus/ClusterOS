using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.Services;
using Hub.Domain;

namespace ClusterOS.Views
{
    public sealed partial class JournalListView : Page
    {
        private readonly JournalService _journalService = new JournalService();

        public JournalListView()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var journals = await _journalService.GetJournalsAsync();
            JournalsListView.ItemsSource = journals;
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Journal journal)
            {
                this.Frame.Navigate(typeof(JournalDateView), journal);
            }
        }
    }
}
