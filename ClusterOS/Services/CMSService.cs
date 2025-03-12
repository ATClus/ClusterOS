using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using ClusterOS.Models;

namespace ClusterOS.Services
{
    public class CMSService
    {
        private readonly HttpClient httpClient = new HttpClient();

        public async Task<List<CategoryModel>> GetCategoriesAsync()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string endpointsJson = localSettings.Values["CMS_Endpoints"]?.ToString();

            if (string.IsNullOrWhiteSpace(endpointsJson))
            {
                throw new Exception("No CMS endpoints are configured. Please configure them in CMS settings.");
            }

            var endpoints = JsonSerializer.Deserialize<List<EndpointModel>>(endpointsJson);
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
            List<CategoryModel> categories = JsonSerializer.Deserialize<List<CategoryModel>>(json);

            if (categories != null)
            {
                categories = categories.Where(c => c.id != 2 && c.id != 3 && c.id != 4).ToList();
            }

            return categories;
        }

        public async Task DeleteArticleAsync(string articleId)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string endpointsJson = localSettings.Values["CMS_Endpoints"]?.ToString();

            if (string.IsNullOrWhiteSpace(endpointsJson))
            {
                throw new Exception("No CMS endpoints are configured. Please configure them in CMS settings.");
            }

            var endpoints = JsonSerializer.Deserialize<List<EndpointModel>>(endpointsJson);
            var deleteArticleEndpoint = endpoints?
                .FirstOrDefault(e =>
                    e.Function.Equals("Article", StringComparison.OrdinalIgnoreCase) &&
                    e.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))?.Link;

            if (string.IsNullOrWhiteSpace(deleteArticleEndpoint))
            {
                throw new Exception("The endpoint for article deletion (DELETE) is not configured. Please configure it in CMS settings.");
            }

            string requestUrl = deleteArticleEndpoint.Contains("{id}")
                ? deleteArticleEndpoint.Replace("{id}", articleId)
                : deleteArticleEndpoint.TrimEnd('/') + "/" + articleId;

            HttpResponseMessage response = await httpClient.DeleteAsync(requestUrl);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ArticleModel>> GetArticlesAsync()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string endpointsJson = localSettings.Values["CMS_Endpoints"]?.ToString();

            if (string.IsNullOrWhiteSpace(endpointsJson))
            {
                throw new Exception("No CMS endpoints are configured. Please configure them in CMS settings.");
            }

            var endpoints = JsonSerializer.Deserialize<List<EndpointModel>>(endpointsJson);
            var articlesEndpoint = endpoints?
                .FirstOrDefault(e =>
                    e.Function.Equals("Articles", StringComparison.OrdinalIgnoreCase) &&
                    e.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))?.Link;

            if (string.IsNullOrWhiteSpace(articlesEndpoint))
            {
                throw new Exception("The endpoint for articles (GET) is not configured. Please configure it in CMS settings.");
            }

            HttpResponseMessage response = await httpClient.GetAsync(articlesEndpoint);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            List<ArticleModel> articles = JsonSerializer.Deserialize<List<ArticleModel>>(json);
            return articles;
        }

        public async Task SaveArticleAsync(ArticleModel article)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string endpointsJson = localSettings.Values["CMS_Endpoints"]?.ToString();

            if (string.IsNullOrWhiteSpace(endpointsJson))
            {
                throw new Exception("No CMS endpoints are configured. Please configure them in CMS settings.");
            }

            var endpoints = JsonSerializer.Deserialize<List<EndpointModel>>(endpointsJson);
            var saveArticleEndpoint = endpoints?
                .FirstOrDefault(e =>
                    e.Function.Equals("Article", StringComparison.OrdinalIgnoreCase) &&
                    e.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))?.Link;

            if (string.IsNullOrWhiteSpace(saveArticleEndpoint))
            {
                throw new Exception("The endpoint for saving articles (POST) is not configured. Please configure it in CMS settings.");
            }

            string articleJson = JsonSerializer.Serialize(article);
            var content = new StringContent(articleJson, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(saveArticleEndpoint, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
