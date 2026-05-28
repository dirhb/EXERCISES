using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace LibraryWSClient
{
    public class ApiClient<T>
    {
        HttpClient httpClient = LibraryHttpClient.Instance;
        UriBuilder uriBuilder = new UriBuilder();

        public string Scheme
        {
            set { this.uriBuilder.Scheme = value; }
        }

        public string Host
        {
            set { this.uriBuilder.Host = value; }
        }

        public int Port
        {
            set { this.uriBuilder.Port = value; }
        }

        public string Path
        {
            set { this.uriBuilder.Path = value; }
        }

        public void AddParameter(string key, string value)
        {
            if (this.uriBuilder.Query == string.Empty)
                this.uriBuilder.Query += "?";
            else
                this.uriBuilder.Query += "&";
            this.uriBuilder.Query += $"{key}={value}";
        }

        public async Task<T> GetAsync()
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Get;
                httpRequest.RequestUri = this.uriBuilder.Uri;

                using (HttpResponseMessage httpResponse =
                    await this.httpClient.SendAsync(httpRequest))
                {
                    // If the request failed, return default (null)
                    if (!httpResponse.IsSuccessStatusCode)
                        return default(T);

                    string result = await httpResponse.Content.ReadAsStringAsync();

                    // If response is empty, return default instead of crashing
                    if (string.IsNullOrWhiteSpace(result))
                        return default(T);

                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.PropertyNameCaseInsensitive = true;

                    T model = JsonSerializer.Deserialize<T>(result, options);
                    return model;
                }
            }
        }

        public async Task<bool> PostAsync(T model)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                string json = JsonSerializer.Serialize<T>(model);
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                httpRequest.Content = content;
                using (HttpResponseMessage responseMessage =
                    await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode == true;
                }
            }
        }
    }
}
