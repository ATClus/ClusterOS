using Microsoft.UI.Xaml.Controls;
using System;
using Hub.Domain;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;

namespace ClusterOS.Views
{
    public sealed partial class TasksView : Page
    {
        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();

        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

        public TasksView()
        {
            this.InitializeComponent();
            TasksRoot.Navigate(typeof(TasksListView));
        }

        private void TasksSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem is SelectorBarItem selectedItem)
            {
                if (selectedItem == DisplayTasksItem)
                {
                    TasksRoot.Navigate(typeof(TasksListView));
                }
                else if (selectedItem == CreateEditTaskItem)
                {
                    TasksRoot.Navigate(typeof(TasksCreateView));
                }
            }
        }
    }
}
