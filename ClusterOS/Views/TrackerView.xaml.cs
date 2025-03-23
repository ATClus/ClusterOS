using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
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
        private bool _dateFilterEnabled;

        public TrackerView()
        {
            InitializeComponent();
            Loaded += TrackerView_Loaded;
        }

        private async void TrackerView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            await LoadDataAsync();
            TrackDataListView.ItemsSource = Items;
            TrackDataListView.LayoutUpdated += TrackDataListView_LayoutUpdated;
        }

        private void TrackDataListView_LayoutUpdated(object sender, object e)
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            TrackDataListView.LayoutUpdated -= TrackDataListView_LayoutUpdated;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync("https://localhost:7131/track-data");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<TrackDataDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dto?.Items != null)
                {
                    foreach (var item in dto.Items)
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(AppTitleFilterTextBox.Text))
                filtered = filtered.Where(i => i.AppTitle.Contains(AppTitleFilterTextBox.Text, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(TabUrlFilterTextBox.Text))
                filtered = filtered.Where(i => i.TabUrl.Contains(TabUrlFilterTextBox.Text, StringComparison.OrdinalIgnoreCase));

            if (_dateFilterEnabled)
            {
                var date = DateFilterPicker.Date.Date;
                filtered = filtered.Where(i => i.Date.Date == date);
            }

            _filteredItems = new ObservableCollection<TrackDataItemModel>(filtered);
            TrackDataListView.ItemsSource = _filteredItems;
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

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
            TrackDataListView.ItemsSource = Items;
        }
    }
}
