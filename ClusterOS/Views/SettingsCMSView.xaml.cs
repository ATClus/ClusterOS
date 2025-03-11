using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace ClusterOS.Views
{
    public class Endpoint
    {
        public string Method { get; set; }
        public string Link { get; set; }
        public string Function { get; set; }
    }

    public sealed partial class SettingsCMSView : Page
    {
        public ObservableCollection<Endpoint> Endpoints { get; set; } = new ObservableCollection<Endpoint>();
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SettingsCMSView()
        {
            this.InitializeComponent();
            lvEndpoints.ItemsSource = Endpoints;
            LoadEndpoints();
        }

        private void LoadEndpoints()
        {
            Endpoints.Clear();
            if (localSettings.Values.TryGetValue("CMS_EndpointCount", out object countObj) &&
                int.TryParse(countObj.ToString(), out int count))
            {
                for (int i = 0; i < count; i++)
                {
                    string method = localSettings.Values[$"CMS_Endpoint_{i}_Method"]?.ToString();
                    string link = localSettings.Values[$"CMS_Endpoint_{i}_Link"]?.ToString();
                    string function = localSettings.Values[$"CMS_Endpoint_{i}_Function"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(method) &&
                        !string.IsNullOrWhiteSpace(link) &&
                        !string.IsNullOrWhiteSpace(function))
                    {
                        Endpoints.Add(new Endpoint { Method = method, Link = link, Function = function });
                    }
                }
            }
        }

        private void AddEndpoint_Click(object sender, RoutedEventArgs e)
        {
            string method = (cmbMethod.SelectedItem as ComboBoxItem)?.Content.ToString();
            string link = txtEndpointLink.Text;
            string function = txtFunction.Text;

            if (string.IsNullOrWhiteSpace(method) ||
                string.IsNullOrWhiteSpace(link) ||
                string.IsNullOrWhiteSpace(function))
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Atenção",
                    Content = "Todos os campos devem ser preenchidos para adicionar um endpoint.",
                    CloseButtonText = "Ok"
                };
                _ = dialog.ShowAsync();
                return;
            }

            Endpoints.Add(new Endpoint { Method = method, Link = link, Function = function });

            cmbMethod.SelectedIndex = -1;
            txtEndpointLink.Text = string.Empty;
            txtFunction.Text = string.Empty;
        }

        private void RemoveEndpoint_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Endpoint endpoint)
            {
                Endpoints.Remove(endpoint);
            }
        }

        private void EditEndpoint_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Endpoint endpoint)
            {
                foreach (var item in cmbMethod.Items)
                {
                    if (item is ComboBoxItem comboItem && comboItem.Content.ToString() == endpoint.Method)
                    {
                        cmbMethod.SelectedItem = item;
                        break;
                    }
                }
                txtEndpointLink.Text = endpoint.Link;
                txtFunction.Text = endpoint.Function;

                Endpoints.Remove(endpoint);
            }
        }

        private void SaveConfigurations_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values["CMS_EndpointCount"] = Endpoints.Count;

            for (int i = 0; i < Endpoints.Count; i++)
            {
                localSettings.Values[$"CMS_Endpoint_{i}_Method"] = Endpoints[i].Method;
                localSettings.Values[$"CMS_Endpoint_{i}_Link"] = Endpoints[i].Link;
                localSettings.Values[$"CMS_Endpoint_{i}_Function"] = Endpoints[i].Function;
            }

            ContentDialog successDialog = new ContentDialog
            {
                Title = "Sucesso",
                Content = "Configurações salvas com sucesso.",
                CloseButtonText = "Ok"
            };
            _ = successDialog.ShowAsync();
        }
    }
}
