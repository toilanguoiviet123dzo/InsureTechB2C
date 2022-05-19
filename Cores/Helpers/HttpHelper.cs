using Cores.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cores.Helpers
{
    public class HttpHelper
    {
        public string BaseAddress { get; set; }
        private readonly IHttpClientFactory _httpClientFactory;
        private string _clientName = "";
        public HttpHelper(string clientName, IHttpClientFactory httpClientFactory)
        {
            _clientName = clientName;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TResult> GetFromJsonAsync<TResult>(string requestUri, string authorizationToken = "", string authorizationScheme = "", Dictionary<string, string> headers = null)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                client.DefaultRequestHeaders.Clear();
                //Authorization
                if (!string.IsNullOrWhiteSpace(authorizationToken) && !string.IsNullOrWhiteSpace(authorizationScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationToken);
                }
                //Header
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                //Reuquest
                return await client.GetFromJsonAsync<TResult>(requestUri);
            }
            catch
            {
                return default(TResult);
            }
        }

        public async Task<TResult> GetAsNewtonsoftJsonAsync<TResult>(string requestUri, string authorizationToken = "", string authorizationScheme = "", Dictionary<string, string> headers = null)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                client.DefaultRequestHeaders.Clear();
                //Authorization
                if (!string.IsNullOrWhiteSpace(authorizationToken) && !string.IsNullOrWhiteSpace(authorizationScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationToken);
                }
                //Header
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                //Reuquest
                var response = await client.GetAsync(requestUri);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                    var sr = new StreamReader(await response.Content.ReadAsStreamAsync());
                    return MyJson.Deserialize<TResult>(sr);
                }
                else
                {
                    return default(TResult);
                }
            }
            catch
            {
                return default(TResult);
            }
        }

        //
        public async Task<TResult> PostAsJsonAsync<T, TResult>(string requestUri, T objData, string authorizationToken = "", string authorizationScheme = "", Dictionary<string, string> headers = null)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                client.DefaultRequestHeaders.Clear();
                //Authorization
                if (!string.IsNullOrWhiteSpace(authorizationToken) && !string.IsNullOrWhiteSpace(authorizationScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationToken);
                }
                //Header
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                //Reuquest
                HttpResponseMessage response = await client.PostAsJsonAsync<T>(requestUri, objData, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
                // return
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadFromJsonAsync<TResult>();
                }
                else
                {
                    return default(TResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(TResult);
            }
        }

        public async Task<TResult> GetAsync<TResult>(string requestUri, string authorizationToken = "", string authorizationScheme = "", Dictionary<string, string> headers = null)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                client.DefaultRequestHeaders.Clear();
                //Authorization
                if (!string.IsNullOrWhiteSpace(authorizationToken) && !string.IsNullOrWhiteSpace(authorizationScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationToken);
                }
                //Header
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                //
                HttpResponseMessage response = await client.GetAsync(requestUri);
                // return
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                    var sr = new StreamReader(await response.Content.ReadAsStreamAsync());
                    return MyJson.Deserialize<TResult>(sr);
                }
                else
                {
                    return default(TResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(TResult);
            }
        }

        public async Task<TResult> PostAsync<T, TResult>(string requestUri, T objData, string authorizationToken = "", string authorizationScheme = "", Dictionary<string, string> headers = null)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                client.DefaultRequestHeaders.Clear();
                //Authorization
                if (!string.IsNullOrWhiteSpace(authorizationToken) && !string.IsNullOrWhiteSpace(authorizationScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationToken);
                }
                //Header
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                //Content
                string json = JsonConvert.SerializeObject(objData, Formatting.Indented);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //
                HttpResponseMessage response = await client.PostAsync(requestUri, content);
                // return
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                    var sr = new StreamReader(await response.Content.ReadAsStreamAsync());
                    return MyJson.Deserialize<TResult>(sr);
                }
                else
                {
                    return default(TResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(TResult);
            }
        }

        public async Task<TResult> PostAsNewtonsoftJsonAsync<T, TResult>(string requestUri, T objData, string authorizationToken = "", string authorizationScheme = "", Dictionary<string, string> headers = null)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                client.DefaultRequestHeaders.Clear();
                //Authorization
                if (!string.IsNullOrWhiteSpace(authorizationToken) && !string.IsNullOrWhiteSpace(authorizationScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationToken);
                }
                //Header
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                //Content
                string json = JsonConvert.SerializeObject(objData, Formatting.Indented);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //
                HttpResponseMessage response = await client.PostAsync(requestUri, content);
                // return
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                    var sr = new StreamReader(await response.Content.ReadAsStreamAsync());
                    return MyJson.Deserialize<TResult>(sr);
                } 
                else
                {
                    return default(TResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(TResult);
            }
        }

        public async Task<TResult> PutAsJsonAsync<T, TResult>(string requestUri, T objData)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            //Create client
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                //Reuquest
                HttpResponseMessage response = await client.PutAsJsonAsync<T>(requestUri, objData);
                response.EnsureSuccessStatusCode();

                // return
                return await response.Content.ReadFromJsonAsync<TResult>();
            }
            catch
            {
                return default(TResult);
            }
        }

        public async Task<TResult> DeleteAsync<TResult>(string requestUri)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            //Create client
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                //Reuquest
                HttpResponseMessage response = await client.DeleteAsync(requestUri);
                response.EnsureSuccessStatusCode();

                // return
                return await response.Content.ReadFromJsonAsync<TResult>();
            }
            catch
            {
                return default(TResult);
            }
        }

        public async Task<byte[]> GetByteArrayAsync(string requestUri)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(byte[]);

            //Create client
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                return await client.GetByteArrayAsync(requestUri);
            }
            catch
            {
                return default(byte[]);
            }
        }


        public async Task<string> GetStringAsync(string requestUri)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(string);

            //Create client
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                return await client.GetStringAsync(requestUri);
            }
            catch
            {
                return default(string);
            }
        }

        public async Task<Stream> GetStreamAsync(string requestUri)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(Stream);

            //Create client
            var client = _httpClientFactory.CreateClient(_clientName);
            //Make request
            try
            {
                return await client.GetStreamAsync(requestUri);
            }
            catch
            {
                return default(Stream);
            }

        }
        //
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            //Create client
            var client = _httpClientFactory.CreateClient(_clientName);
            //
            return await client.SendAsync(request);
        }
        //
        public string HttpPostJson(string link, string postJsonString)
        {
            try
            {
                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(link);
                var postData = postJsonString;
                var data = Encoding.UTF8.GetBytes(postData);
                httpWReq.ProtocolVersion = HttpVersion.Version11;
                httpWReq.Method = "POST";
                httpWReq.ContentType = "application/json";
                httpWReq.ContentLength = data.Length;
                httpWReq.Timeout = 15000;
                Stream stream = httpWReq.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
                string jsonresponse = "";
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string temp = null;
                    while ((temp = reader.ReadLine()) != null)
                    {
                        jsonresponse += temp;
                    }
                }
                return jsonresponse;
            }
            catch (WebException e)
            {
                return e.Message;
            }
        }

    }//End class
}//End namespace
