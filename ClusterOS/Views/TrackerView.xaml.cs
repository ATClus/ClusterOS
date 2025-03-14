using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.Models;
using Hub.Application;

namespace ClusterOS.Views
{
    public sealed partial class TrackerView : Page
    {
        public ObservableCollection<TrackDataItemModel> Items { get; } = new ObservableCollection<TrackDataItemModel>();
        private ObservableCollection<TrackDataItemModel> _filteredItems = new ObservableCollection<TrackDataItemModel>();
        private bool _dateFilterEnabled = false;

        public TrackerView()
        {
            this.InitializeComponent();
            this.Loaded += TrackerView_Loaded;
        }

        private async void TrackerView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync("https://localhost:7131/track-data");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var trackDataDto = JsonSerializer.Deserialize<TrackDataDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (trackDataDto?.Items != null)
                    {
                        foreach (var item in trackDataDto.Items)
                        {
                            Items.Add(new TrackDataItemModel
                            {
                                Id = item.Id,
                                AppTitle = item.AppTitle,
                                TabUrl = item.TabUrl,
                                Date = item.Date,
                                Duration = item.Duration
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }

            TrackDataListView.ItemsSource = Items;
        }

        private void ApplyFilters()
        {
            var filtered = Items.AsEnumerable();

            if (!string.IsNullOrEmpty(AppTitleFilterTextBox.Text))
            {
                filtered = filtered.Where(item => item.AppTitle.Contains(AppTitleFilterTextBox.Text, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(TabUrlFilterTextBox.Text))
            {
                filtered = filtered.Where(item => item.TabUrl.Contains(TabUrlFilterTextBox.Text, StringComparison.OrdinalIgnoreCase));
            }

            if (_dateFilterEnabled)
            {
                var selectedDate = DateFilterPicker.Date.Date;
                filtered = filtered.Where(item => item.Date.Date == selectedDate);
            }

            _filteredItems = new ObservableCollection<TrackDataItemModel>(filtered);
            TrackDataListView.ItemsSource = _filteredItems;
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilterPicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            _dateFilterEnabled = true;
            ApplyFilters();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            AppTitleFilterTextBox.Text = string.Empty;
            TabUrlFilterTextBox.Text = string.Empty;
            _dateFilterEnabled = false;
            DateFilterPicker.Date = DateTimeOffset.Now;
            ApplyFilters();
        }
    }
}
