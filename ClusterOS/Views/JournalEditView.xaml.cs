using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Hub.Domain;
using ClusterOS.Services;
using System.Linq;

namespace ClusterOS.Views
{
    public sealed partial class JournalEditView : Page
    {
        private readonly DispatcherTimer autoSaveTimer;
        private bool contentChanged = false;
        private bool isSaving = false;
        private Journal currentJournal;
        private readonly JournalService journalService = new JournalService();
        private string lastSavedContent = string.Empty;

        public JournalEditView()
        {
            this.InitializeComponent();
            this.Loaded += JournalEditView_Loaded;

            autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Start();
        }

        private async void JournalEditView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime today = DateTime.UtcNow.Date;
                JournalTitleTextBlock.Text = today.ToString("d");

                Journal todayJournal = null;
                try
                {
                    string endpoint = $"{new JournalService().baseUrl}/journals/bydate/{today:yyyy-MM-dd}";
                    todayJournal = await journalService.GetJournalByDateAsync(today);
                }
                catch
                {
                    var journals = await journalService.GetJournalsAsync();
                    todayJournal = journals.FirstOrDefault(j =>
                        j.Created.Year == today.Year &&
                        j.Created.Month == today.Month &&
                        j.Created.Day == today.Day);
                }

                if (todayJournal != null)
                {
                    currentJournal = todayJournal;
                    lastSavedContent = currentJournal.Content;

                    if (!string.IsNullOrEmpty(currentJournal.Content))
                    {
                        JournalRichEditBox.Document.SetText(TextSetOptions.None, currentJournal.Content);
                    }
                }
                else
                {
                    currentJournal = new Journal(string.Empty)
                    {
                        Created = today
                    };
                    lastSavedContent = string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading journal: {ex.Message}");
                AutoSaveStatusTextBlock.Text = "Error loading journal";
                AutoSaveStatusTextBlock.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                AutoSaveStatusTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private async void AutoSaveTimer_Tick(object sender, object e)
        {
            if (!contentChanged || isSaving)
                return;

            try
            {
                string currentContent = await GetRichEditBoxTextAsync(JournalRichEditBox);

                if (string.Equals(currentContent, lastSavedContent))
                {
                    contentChanged = false;
                    return;
                }

                isSaving = true;
                contentChanged = false;

                AutoSaveStatusTextBlock.Text = "Saving...";
                AutoSaveStatusTextBlock.Visibility = Visibility.Visible;

                currentJournal.Content = currentContent;

                if (currentJournal.Id == 0)
                {
                    try
                    {
                        var today = DateTime.UtcNow.Date;
                        var journals = await journalService.GetJournalsAsync();
                        var existingJournal = journals.FirstOrDefault(j =>
                            j.Created.Year == today.Year &&
                            j.Created.Month == today.Month &&
                            j.Created.Day == today.Day);

                        if (existingJournal != null)
                        {
                            currentJournal = existingJournal;
                            currentJournal.Content = currentContent;
                            await journalService.UpdateJournalAsync(currentJournal.Id, currentJournal);
                        }
                        else
                        {
                            Journal savedJournal = await journalService.SaveJournalAsync(currentJournal);
                            if (savedJournal != null && savedJournal.Id != 0)
                            {
                                currentJournal = savedJournal;
                            }
                            else
                            {
                                throw new Exception("Invalid return from creation service.");
                            }
                        }

                        lastSavedContent = currentContent;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Create journal error: {ex}");
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        await journalService.UpdateJournalAsync(currentJournal.Id, currentJournal);
                        lastSavedContent = currentContent;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Update journal error: {ex}");
                        throw;
                    }
                }

                AutoSaveStatusTextBlock.Text = "Saved";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoSave Error: {ex}");
                AutoSaveStatusTextBlock.Text = "Save error";
                contentChanged = true;
            }
            finally
            {
                await Task.Delay(1500);
                AutoSaveStatusTextBlock.Visibility = Visibility.Collapsed;
                isSaving = false;
            }
        }

        private Task<string> GetRichEditBoxTextAsync(RichEditBox richEditBox)
        {
            richEditBox.Document.GetText(TextGetOptions.None, out string text);
            return Task.FromResult(text);
        }

        private void JournalRichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            contentChanged = true;
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = JournalRichEditBox.Document.Selection;
            bool isBold = selection.CharacterFormat.Bold == FormatEffect.On;
            selection.CharacterFormat.Bold = isBold ? FormatEffect.Off : FormatEffect.On;
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = JournalRichEditBox.Document.Selection;
            bool isItalic = selection.CharacterFormat.Italic == FormatEffect.On;
            selection.CharacterFormat.Italic = isItalic ? FormatEffect.Off : FormatEffect.On;
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (double.TryParse(selectedItem.Content.ToString(), out double newSize))
                {
                    if (JournalRichEditBox?.Document == null)
                        return;

                    var selection = JournalRichEditBox.Document.Selection;
                    if (selection != null)
                    {
                        selection.CharacterFormat.Size = (float)newSize;
                    }
                }
            }
        }

        private void BulletListButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = JournalRichEditBox.Document.Selection;
            selection.ParagraphFormat.ListType = selection.ParagraphFormat.ListType == MarkerType.Bullet
                ? MarkerType.None
                : MarkerType.Bullet;
        }
    }
}