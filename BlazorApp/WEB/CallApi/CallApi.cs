using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using Newtonsoft.Json;
namespace WEB.CallApi
{
    public class CallApi : IcallApi
    {
        private readonly HttpClient _httpClient;

        public CallApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> PostAsync(string url, object body = null, string bearerToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            var json = JsonConvert.SerializeObject(body ?? new { });
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error calling API: {ex.Message}";
            }
        }

    }
}
