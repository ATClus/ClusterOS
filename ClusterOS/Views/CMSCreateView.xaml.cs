using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClusterOS.Models;
using System.Linq;

namespace ClusterOS.Views
{
    public sealed partial class CMSCreateView : Page
    {
        private readonly CMSService cmsService = new CMSService();
        private List<CategoryModel> _categories;

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
                List<CategoryModel> categories = await cmsService.GetCategoriesAsync();
                _categories = categories;
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

        private async void SavePost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text) ||
                    string.IsNullOrWhiteSpace(txtSummary.Text) ||
                    string.IsNullOrWhiteSpace(txtContentPT.Text) ||
                    string.IsNullOrWhiteSpace(txtContentEN.Text) ||
                    string.IsNullOrWhiteSpace(txtAuthor.Text) ||
                    cmbCategory.SelectedItem == null)
                {
                    ContentDialog validationDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "All fields must be filled.",
                        CloseButtonText = "Ok",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }

                string selectedCategoryName = ((ComboBoxItem)cmbCategory.SelectedItem).Content.ToString();

                int categoryId = 0;
                if (_categories != null)
                {
                    var category = _categories.FirstOrDefault(c =>
                        c.name.Equals(selectedCategoryName, StringComparison.OrdinalIgnoreCase));
                    if (category != null)
                    {
                        categoryId = category.id;
                    }
                }

                if (categoryId == 0)
                {
                    ContentDialog catErrorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Invalid category selected.",
                        CloseButtonText = "Ok",
                        XamlRoot = this.XamlRoot
                    };
                    await catErrorDialog.ShowAsync();
                    return;
                }

                var markdownParser = new MarkdownLib.Parser.MarkdownParser();
                string parsedContentPT = markdownParser.Parse(txtContentPT.Text);
                string parsedContentEN = markdownParser.Parse(txtContentEN.Text);

                ArticleModel article = new ArticleModel
                {
                    title = txtTitle.Text,
                    summary = txtSummary.Text,
                    contentPT = parsedContentPT,
                    contentEN = parsedContentEN,
                    author = txtAuthor.Text,
                    categoryId = categoryId,
                    isPublished = false,
                    highlight = tglHighlight.IsOn
                };

                await cmsService.SaveArticleAsync(article);

                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Post saved successfully.",
                    CloseButtonText = "Ok",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
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
    }
}
