using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hub.Domain;
namespace ClusterOS.Services
{
    public class JournalService
    {
        private readonly HttpClient httpClient = new HttpClient();
        public readonly string baseUrl;

        public JournalService()
        {
            var port = System.Diagnostics.Debugger.IsAttached ? "7132" : "7131";
            baseUrl = $"https://localhost:{port}";
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<Journal>> GetJournalsAsync()
        {
            try
            {
                string journalsEndpoint = $"{baseUrl}/journals";
                HttpResponseMessage response = await httpClient.GetAsync(journalsEndpoint);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                List<Journal> journals = JsonSerializer.Deserialize<List<Journal>>(json, options);
                return journals ?? new List<Journal>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetJournalsAsync: {ex}");
                return new List<Journal>();
            }
        }

        public async Task<Journal> GetJournalByIdAsync(int id)
        {
            string requestUrl = $"{baseUrl}/journals/{id}";
            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Journal journal = JsonSerializer.Deserialize<Journal>(json, options);
            return journal;
        }

        public async Task<Journal> GetJournalByDateAsync(DateTime date)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            string requestUrl = $"{baseUrl}/journals/bydate/{formattedDate}";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Journal journal = JsonSerializer.Deserialize<Journal>(json, options);
                return journal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Journal> SaveJournalAsync(Journal journal)
        {
            string saveJournalEndpoint = $"{baseUrl}/journals";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            string journalJson = JsonSerializer.Serialize(journal, options);
            var content = new StringContent(journalJson, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(saveJournalEndpoint, content);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                Journal savedJournal = JsonSerializer.Deserialize<Journal>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return savedJournal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SaveJournalAsync: {ex}");
                throw;
            }
        }

        public async Task UpdateJournalAsync(int id, Journal journal)
        {
            string requestUrl = $"{baseUrl}/journals/{id}";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            string journalJson = JsonSerializer.Serialize(journal, options);
            var content = new StringContent(journalJson, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.PutAsync(requestUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateJournalAsync: {ex}");
                throw;
            }
        }

        public async Task DeleteJournalAsync(int id)
        {
            string requestUrl = $"{baseUrl}/journals/{id}";
            HttpResponseMessage response = await httpClient.DeleteAsync(requestUrl);
            response.EnsureSuccessStatusCode();
        }
    }
}