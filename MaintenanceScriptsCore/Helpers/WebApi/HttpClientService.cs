using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MaintenanceScriptsCore.Helpers.WebApi
{
    public interface IHttpClientService
    {
        Task<T> GetAsync<T>(string url, string bearerToken);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content, string bearerToken);
        Task<HttpResponseMessage> PatchAsync(string url, HttpContent content, string bearerToken);
        Task<HttpResponseMessage> DeleteAsync(string url, string bearerToken);
    }

    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<T> GetAsync<T>(string url, string bearerToken)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            using (var response = await _httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode(); //HttpRequestException

                using (var content = response.Content)
                {
                    var result = await content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<T>(result);
                }
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content, string bearerToken)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            return await _httpClient.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content, string bearerToken)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            return await _httpClient.PatchAsync(url, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url, string bearerToken)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            return await _httpClient.DeleteAsync(url);
        }
    }
}