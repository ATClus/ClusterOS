using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Text.Json;

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
            if (localSettings.Values.TryGetValue("CMS_Endpoints", out object jsonObj))
            {
                string json = jsonObj?.ToString();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var endpointsFromSettings = JsonSerializer.Deserialize<ObservableCollection<Endpoint>>(json);
                    if (endpointsFromSettings != null)
                    {
                        foreach (var endpoint in endpointsFromSettings)
                        {
                            Endpoints.Add(endpoint);
                        }
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
                    Title = "Attention",
                    Content = "All fields must be filled to add an endpoint.",
                    CloseButtonText = "Ok",
                    XamlRoot = this.XamlRoot
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
            string json = JsonSerializer.Serialize(Endpoints);
            localSettings.Values["CMS_Endpoints"] = json;

            ContentDialog successDialog = new ContentDialog
            {
                Title = "Success",
                Content = "Configurations saved successfully.",
                CloseButtonText = "Ok",
                XamlRoot = this.XamlRoot
            };
            _ = successDialog.ShowAsync();
        }
    }
}
