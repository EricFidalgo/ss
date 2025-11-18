using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Library.eCommerce.Utilities
{
    public class WebRequestHandler
    {
        private string host = "localhost";
        private string port = "7009"; // Make sure this matches your API's port
        private HttpClient Client { get; }

        public WebRequestHandler()
        {
            Client = new HttpClient();
        }

        public async Task<string> Get(string url)
        {
            var fullUrl = $"https://{host}:{port}{url}";
            try
            {
                using (var client = new HttpClient())
                {
                    return await client.GetStringAsync(fullUrl).ConfigureAwait(false);
                }
            }
            catch (Exception) { return null; }
        }

        public async Task<string> Delete(string url)
        {
            var fullUrl = $"https://{host}:{port}{url}";
            try
            {
                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Delete, fullUrl))
                    {
                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return "SUCCESS";
                            }
                            return "ERROR";
                        }
                    }
                }
            }
            catch (Exception) { return null; }
        }

        public async Task<string> Post(string url, object obj)
        {
            var fullUrl = $"https://{host}:{port}{url}";
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, fullUrl))
                {
                    var json = JsonConvert.SerializeObject(obj);
                    using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        request.Content = stringContent;

                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return await response.Content.ReadAsStringAsync();
                            }
                            return "ERROR";
                        }
                    }
                }
            }
        }

        // Added PUT for Update functionality
        public async Task<string> Put(string url, object obj)
        {
            var fullUrl = $"https://{host}:{port}{url}";
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, fullUrl))
                {
                    var json = JsonConvert.SerializeObject(obj);
                    using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        request.Content = stringContent;

                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return await response.Content.ReadAsStringAsync();
                            }
                            return "ERROR";
                        }
                    }
                }
            }
        }
    }
}