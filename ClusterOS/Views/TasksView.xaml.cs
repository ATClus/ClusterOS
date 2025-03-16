using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using ClusterOS.Services;
using ClusterOS.Models;

namespace ClusterOS.Views
{
    public sealed partial class TasksView : Page
    {
        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();
        private readonly TaskService _taskService = new TaskService();

        public TasksView()
        {
            this.InitializeComponent();
            LoadTasksAsync();
            TasksRoot.Navigate(typeof(TasksListView));
        }

        private async void LoadTasksAsync()
        {
            try
            {
                var tasks = await _taskService.GetTasksAsync();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tasks: {ex.Message}");
            }
        }

        private void CreateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            TasksRoot.Navigate(typeof(TasksCreateView));
        }
    }
}
