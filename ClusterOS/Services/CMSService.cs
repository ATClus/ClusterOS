using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClusterOS.Services
{
    // Classe representando a categoria conforme resposta da API
    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool isDeleted { get; set; }
    }

    // Classe representando o endpoint configurado
    public class Endpoint
    {
        public string Method { get; set; }
        public string Link { get; set; }
        public string Function { get; set; }
    }

    public class CMSService
    {
        private readonly HttpClient httpClient = new HttpClient();

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string endpointsJson = localSettings.Values["CMS_Endpoints"]?.ToString();

            if (string.IsNullOrWhiteSpace(endpointsJson))
            {
                throw new Exception("No CMS endpoints are configured. Please configure them in CMS settings.");
            }

            var endpoints = JsonSerializer.Deserialize<List<Endpoint>>(endpointsJson);
            var categoriesEndpoint = endpoints?
                .FirstOrDefault(e =>
                    e.Function.Equals("Categories", StringComparison.OrdinalIgnoreCase) &&
                    e.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))?.Link;

            if (string.IsNullOrWhiteSpace(categoriesEndpoint))
            {
                throw new Exception("The endpoint for categories (GET) is not configured. Please configure it in CMS settings.");
            }

            HttpResponseMessage response = await httpClient.GetAsync(categoriesEndpoint);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            List<Category> categories = JsonSerializer.Deserialize<List<Category>>(json);
            return categories;
        }
    }
}
