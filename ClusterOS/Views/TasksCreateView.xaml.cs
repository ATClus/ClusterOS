using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ClusterOS.Services;
using ClusterOS.Models;

namespace ClusterOS.Views
{
    public sealed partial class TasksCreateView : Page
    {
        private readonly TaskService _taskService = new TaskService();
        private int? editingTaskId = null;

        public TasksCreateView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int taskId)
            {
                try
                {
                    var task = await _taskService.GetTaskByIdAsync(taskId);
                    TitleTextBox.Text = task.title;
                    DescriptionTextBox.Text = task.description;
                    PriorityComboBox.SelectedIndex = (int)task.priority;
                    editingTaskId = taskId;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading the task: {ex.Message}");
                }
            }
        }

        private async void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (editingTaskId.HasValue)
                {
                    var task = await _taskService.GetTaskByIdAsync(editingTaskId.Value);
                    task.Update(TitleTextBox.Text, DescriptionTextBox.Text, (Priority)PriorityComboBox.SelectedIndex);
                    await _taskService.UpdateTaskAsync(editingTaskId.Value, task);
                }
                else
                {
                    var task = new TaskItem(TitleTextBox.Text, DescriptionTextBox.Text, (Priority)PriorityComboBox.SelectedIndex);
                    await _taskService.SaveTaskAsync(task);
                }

                if (Frame.CanGoBack)
                    Frame.GoBack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving the task: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
