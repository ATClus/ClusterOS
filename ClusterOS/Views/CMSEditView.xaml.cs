using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ClusterOS.Models;
using ClusterOS.Services;

namespace ClusterOS.Views
{
    public sealed partial class CMSEditView : Page
    {
        // Lista filtrada para exibição na UI
        public ObservableCollection<ArticleModel> Articles { get; set; } = new ObservableCollection<ArticleModel>();
        // Lista completa obtida da API
        private List<ArticleModel> AllArticles { get; set; } = new List<ArticleModel>();
        // Lista de categorias carregadas
        private List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();

        private readonly CMSService cmsService = new CMSService();

        public CMSEditView()
        {
            this.InitializeComponent();
            lvPosts.ItemsSource = Articles;
            this.Loaded += CMSEditView_Loaded;
        }

        private async void CMSEditView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Categories = await cmsService.GetCategoriesAsync();
                cmbEditCategory.Items.Clear();
                foreach (var cat in Categories)
                {
                    cmbEditCategory.Items.Add(new ComboBoxItem { Content = cat.name });
                }

                AllArticles = await cmsService.GetArticlesAsync();
                FilterArticles();
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "Ok",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private void FilterArticles()
        {
            Articles.Clear();

            if (cmbEditCategory.SelectedItem is not ComboBoxItem selectedItem)
            {
                foreach (var article in AllArticles)
                {
                    Articles.Add(article);
                }
            }
            else
            {
                string selectedCategoryName = selectedItem.Content.ToString();
                var category = Categories.FirstOrDefault(c => c.name.Equals(selectedCategoryName, StringComparison.OrdinalIgnoreCase));
                if (category != null)
                {
                    var filtered = AllArticles.Where(a => a.categoryId == category.id);
                    foreach (var article in filtered)
                    {
                        Articles.Add(article);
                    }
                }
            }
        }

        private void cmbEditCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterArticles();
        }

        private async void EditPost_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ArticleModel article)
            {
                ContentDialog editDialog = new ContentDialog
                {
                    Title = "Edit Article",
                    Content = $"Edit article: {article.title}",
                    CloseButtonText = "Ok",
                    XamlRoot = this.XamlRoot
                };
                await editDialog.ShowAsync();
            }
        }

        private async void RemovePost_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ArticleModel article)
            {
                try
                {
                    await cmsService.DeleteArticleAsync(article.id);
                    AllArticles.Remove(article);
                    FilterArticles();
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to remove article: {ex.Message}",
                        CloseButtonText = "Ok",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}
