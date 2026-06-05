using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibraryWSClient
{
    public class ApiClient<T>
    {
        HttpClient httpClient = LibraryHttpClient.Instance;
        UriBuilder uriBuilder = new UriBuilder();

        public string Scheme
        {
            set
            {
                this.uriBuilder.Scheme = value;
            }
        }

        public string Host
        {
            set
            {
                this.uriBuilder.Host = value;
            }
        }
        public int Port
        {
            set
            {
                this.uriBuilder.Port = value;
            }
        }
        public string Path
        {
            set
            {
                this.uriBuilder.Path = value;
            }
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
                    if (httpResponse.IsSuccessStatusCode == true)
                    {
                        string result = await httpResponse.Content.ReadAsStringAsync();
                        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
                        T model = JsonSerializer.Deserialize<T>(result, jsonSerializerOptions);
                        return model;
                    }
                    return default(T);
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
                StringContent content = new StringContent(json);
                httpRequest.Content = content;
                using (HttpResponseMessage responseMessage =
                      await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode == true;

                }

            }
        }


        public async Task<bool> PostAsync(T model, Stream file)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                MultipartFormDataContent multipartFormData =
                                         new MultipartFormDataContent();
                string json = JsonSerializer.Serialize<T>(model);
                StringContent modelContent = new StringContent(json);
                multipartFormData.Add(modelContent, "model");
                StreamContent streamContent = new StreamContent(file);
                multipartFormData.Add(streamContent, "file", "file");
                httpRequest.Content = multipartFormData;
                using (HttpResponseMessage responseMessage =
                      await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode == true;

                }

            }
        }


        public async Task<bool> PostAsync(T model, List<Stream> files)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                MultipartFormDataContent multipartFormData =
                                         new MultipartFormDataContent();
                string json = JsonSerializer.Serialize<T>(model);
                StringContent modelContent = new StringContent(json);
                multipartFormData.Add(modelContent, "model");
                foreach (FileStream fileStream in files)
                {
                    StreamContent streamContent = new StreamContent(fileStream);
                    multipartFormData.Add(streamContent, "file", "file");
                }
                httpRequest.Content = multipartFormData;
                using (HttpResponseMessage responseMessage =
                      await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode == true;

                }

            }
        }



        public async Task<TRequest> PostReturnValueAsync<TRequest, TResponse>(TResponse model)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                string json = JsonSerializer.Serialize<TResponse>(model);
                StringContent content = new StringContent(json);
                httpRequest.Content = content;
                using (HttpResponseMessage responseMessage =
                      await this.httpClient.SendAsync(httpRequest))
                {
                    bool ok = responseMessage.IsSuccessStatusCode;
                    if (ok == true)
                    {
                        string result = await responseMessage.Content.ReadAsStringAsync();
                        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
                        TRequest value = JsonSerializer.Deserialize<TRequest>(result, jsonSerializerOptions);
                    }
                    return default(TRequest);
                }

            }
        }
    }
}