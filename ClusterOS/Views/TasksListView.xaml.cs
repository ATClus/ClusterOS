using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ClusterOS.Models;
using ClusterOS.Services;

namespace ClusterOS.Views
{
    public sealed partial class TasksListView : Page
    {
        private readonly TaskService _taskService = new TaskService();
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

        public TasksListView()
        {
            this.InitializeComponent();
            this.DataContext = this;
            LoadTasks();
        }

        private async void LoadTasks()
        {
            try
            {
                var tasks = await _taskService.GetTasksAsync();

                System.Diagnostics.Debug.WriteLine($"Retrieved {tasks?.Count ?? 0} tasks");
                foreach (var task in tasks ?? Enumerable.Empty<TaskItem>())
                    System.Diagnostics.Debug.WriteLine($"Task: {task.id}, {task.title}");

                Tasks.Clear();
                if (tasks != null)
                {
                    foreach (var task in tasks)
                        Tasks.Add(task);
                }

                TasksListViewPresentation.ItemsSource = null;
                TasksListViewPresentation.ItemsSource = Tasks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadTasks: {ex.Message}");
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int taskId)
            {
                Frame.Navigate(typeof(TasksCreateView), taskId);
            }
        }

        private async void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int taskId)
            {
                try
                {
                    await _taskService.DeleteTaskAsync(taskId);
                    var taskToRemove = Tasks.FirstOrDefault(t => t.id == taskId);
                    if (taskToRemove != null)
                        Tasks.Remove(taskToRemove);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to delete the task");

                }
            }
        }
    }
}
