using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Printing;

namespace ClusterOS.Views
{
    public sealed partial class SettingsView : Page
    {
        private int previousSelectedItemIndex = 0;

        public SettingsView()
        {
            this.InitializeComponent();
        }

        private void SelectorBarChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedItemIndex = sender.Items.IndexOf(selectedItem);
            System.Type pageType;

            switch (currentSelectedItemIndex)
            {
                case 5:
                    pageType = typeof(SettingsCMSView);
                    break;
                default:
                    pageType = typeof(SettingsGeneralView);
                    break;
            }

            var slideNavigationTransitionEffect = currentSelectedItemIndex - previousSelectedItemIndex > 0
                ? SlideNavigationTransitionEffect.FromRight
                : SlideNavigationTransitionEffect.FromLeft;

            SettingsRoot.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });
            previousSelectedItemIndex = currentSelectedItemIndex;
        }
    }
}
