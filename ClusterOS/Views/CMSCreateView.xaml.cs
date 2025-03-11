using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClusterOS.Views
{
    public sealed partial class CMSCreateView : Page
    {
        private readonly CMSService cmsService = new CMSService();

        public CMSCreateView()
        {
            this.InitializeComponent();
            this.Loaded += CMSCreateView_Loaded;
        }

        private async void CMSCreateView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                List<Category> categories = await cmsService.GetCategoriesAsync();
                cmbCategory.Items.Clear();
                foreach (var category in categories)
                {
                    cmbCategory.Items.Add(new ComboBoxItem { Content = category.name });
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "Ok",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void SavePost_Click(object sender, RoutedEventArgs e)
        {
            // Implemente a lógica para salvar o post aqui.
            // Exemplo: ler os valores dos campos, validar e enviar para o endpoint configurado.

            ContentDialog dialog = new ContentDialog
            {
                Title = "Success",
                Content = "Post saved successfully.",
                CloseButtonText = "Ok",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }
    }
}
