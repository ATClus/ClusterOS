using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace ClusterOS.Views
{
    public class Post
    {
        public string Title { get; set; }
    }

    public sealed partial class CMSEditView : Page
    {
        public ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>();

        public CMSEditView()
        {
            this.InitializeComponent();
            lvPosts.ItemsSource = Posts;
        }

        private void cmbEditCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Posts.Clear();
            string selectedCategory = (cmbEditCategory.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                Posts.Add(new Post { Title = $"Post 1 em {selectedCategory}" });
                Posts.Add(new Post { Title = $"Post 2 em {selectedCategory}" });
            }
        }

        private void EditPost_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Post post)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Editar Post",
                    Content = $"Editar post: {post.Title}",
                    CloseButtonText = "Ok"
                };
                _ = dialog.ShowAsync();
            }
        }

        private void RemovePost_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Post post)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Editar Post",
                    Content = $"Editar post: {post.Title}",
                    CloseButtonText = "Ok"
                };
                _ = dialog.ShowAsync();
            }
        }
    }
}
