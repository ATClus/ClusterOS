using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClusterOS.Views
{
    public sealed partial class JournalView : Page
    {
        public JournalView()
        {
            this.InitializeComponent();
            JournalRoot.Navigate(typeof(JournalEditView));
        }

        private void JournalSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (JournalSelectorBar.SelectedItem is SelectorBarItem selectedItem)
            {
                if (selectedItem == JournalViewItem)
                {
                    JournalRoot.Navigate(typeof(JournalEditView));
                }
                else if (selectedItem == JournalListItem)
                {
                    JournalRoot.Navigate(typeof(JournalListView));
                }
            }
        }
    }
}
