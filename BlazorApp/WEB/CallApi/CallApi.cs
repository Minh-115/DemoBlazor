using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
namespace WEB.CallApi
{
    public class CallApi
    {
        private readonly HttpClient _httpClient;

        public CallApi()
        {
            _httpClient = new HttpClient();
        }
        public async Task<string> GetAsync(string url, object queryParams = null, string bearerToken = null)
        {
            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            // Convert object to query string nếu có queryParams
            if (queryParams != null)
            {
                var queryString = ToQueryString(queryParams);
                url = $"{url}?{queryString}";
            }

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw exception if not success
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error calling API: {ex.Message}";
            }
        }
        private string ToQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select $"{HttpUtility.UrlEncode(p.Name)}={HttpUtility.UrlEncode(p.GetValue(obj, null)?.ToString())}";

            return string.Join("&", properties);
        }

    }
}
