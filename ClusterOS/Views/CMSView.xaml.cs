using Microsoft.UI.Xaml.Controls;

namespace ClusterOS.Views
{
    public sealed partial class CMSView : Page
    {
        public CMSView()
        {
            this.InitializeComponent();
            cmsRoot.Navigate(typeof(CMSCreateView));
        }

        private void CmsSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            if (selectedItem == CreateItem)
            {
                cmsRoot.Navigate(typeof(CMSCreateView));
            }
            else if (selectedItem == EditItem)
            {
                cmsRoot.Navigate(typeof(CMSEditView));
            }
        }
    }
}
