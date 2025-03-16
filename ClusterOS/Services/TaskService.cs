using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClusterOS.Models;

namespace ClusterOS.Services
{
    public class TaskService
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string baseUrl;

        public TaskService()
        {
            var port = System.Diagnostics.Debugger.IsAttached ? "7132" : "7131";
            baseUrl = $"https://localhost:{port}";
        }

        public async Task<List<TaskItem>> GetTasksAsync()
        {
            string tasksEndpoint = $"{baseUrl}/tasks";
            HttpResponseMessage response = await httpClient.GetAsync(tasksEndpoint);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<TaskItem> tasks = JsonSerializer.Deserialize<List<TaskItem>>(json, options);
            return tasks;
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            string requestUrl = $"{baseUrl}/tasks/{id}";
            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            TaskItem task = JsonSerializer.Deserialize<TaskItem>(json, options);
            return task;
        }

        public async Task SaveTaskAsync(TaskItem task)
        {
            string saveTaskEndpoint = $"{baseUrl}/tasks";
            string taskJson = JsonSerializer.Serialize(task);
            var content = new StringContent(taskJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(saveTaskEndpoint, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateTaskAsync(int id, TaskItem task)
        {
            string requestUrl = $"{baseUrl}/tasks/{id}";
            string taskJson = JsonSerializer.Serialize(task);
            var content = new StringContent(taskJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PutAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTaskAsync(int id)
        {
            string requestUrl = $"{baseUrl}/tasks/{id}";
            HttpResponseMessage response = await httpClient.DeleteAsync(requestUrl);
            response.EnsureSuccessStatusCode();
        }
    }
}
