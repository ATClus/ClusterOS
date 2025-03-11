using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClusterOS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CMSCreateView : Page
    {
        public CMSCreateView()
        {
            this.InitializeComponent();
        }

        private void SavePost_Click(object sender, RoutedEventArgs e)
        {
            // Leitura dos dados dos campos
            string title = txtTitle.Text;
            string category = (cmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString();
            string summary = txtSummary.Text;
            string contentPT = txtContentPT.Text;
            string contentEN = txtContentEN.Text;
            string author = txtAuthor.Text;

            // TODO: Integração com os endpoints configurados
            // Exemplo: chamar o endpoint de criação com os dados coletados

            ContentDialog dialog = new ContentDialog
            {
                Title = "Sucesso",
                Content = "Post salvo com sucesso.",
                CloseButtonText = "Ok"
            };
            _ = dialog.ShowAsync();

            // Opcional: limpar os campos ou navegar para outra view
        }
    }
}
