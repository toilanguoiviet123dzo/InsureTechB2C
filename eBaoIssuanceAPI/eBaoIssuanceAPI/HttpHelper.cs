using Microsoft.IdentityModel.Clients.ActiveDirectory;
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

namespace Insurtech.Common
{
    public class HttpHelper
    {
        public string BaseAddress { get; set; }
        private string bearerToken = "";
        private string grantCode = "";
        private readonly IHttpClientFactory _httpClientFactory = null;
        private string _clientName = "";
        public HttpHelper(string clientName, IHttpClientFactory httpClientFactory)
        {
            _clientName = clientName;
            _httpClientFactory = httpClientFactory;
        }
        public HttpHelper()
        {
            _clientName = "";
            _httpClientFactory = null;
        }
        /// <summary>
        /// Create http client
        /// </summary>
        /// <returns></returns>
        //public HttpClient Create_HttpClient()
        //{
        //    HttpClient client;
        //    if (_httpClientFactory == null)
        //    {
        //        client = new HttpClient();
        //    }
        //    else
        //    {
        //        client = _httpClientFactory.CreateClient(_clientName);
        //    }
        //    return client;
        //}

        public void SetToken(string token)
        {
            bearerToken = token;
        }
        public void SetGrantCode(string code)
        {
            grantCode = code;
        }

        public async Task<TResult> GetFromJsonAsync<TResult>(string requestUri)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
            //Make request
            try
            {
                //Reuquest
                return await client.GetFromJsonAsync<TResult>(requestUri);
            }
            catch
            {
                return default(TResult);
            }
        }

        /// <summary>
        /// Post data in body json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="objData"></param>
        /// <returns></returns>
        public async Task<TResult> PostAsJsonAsync<T, TResult>(string requestUri, T objData)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
            //Make request
            try
            {
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


        /// <summary>
        /// Post data in form
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="objData"></param>
        /// <returns></returns>
        public async Task<TResult> PostAsFormDataAsync<TResult>(string requestUri, Dictionary<string, string> data)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
            //Make request
            try
            {
                //Form content
                var formContent = new FormUrlEncodedContent(data);

                //Reuquest
                HttpResponseMessage response = await client.PostAsync(requestUri, formContent);
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

        public async Task<TResult> PostAsNewtonsoftJsonAsync<T, TResult>(string requestUri, T objData)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(TResult);
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
            if (bearerToken != "")
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
            if (grantCode != "")
            {
                //client.DefaultRequestHeaders.Authorization. = new AuthenticationHeaderValue("grantCode", grantCode);
                client.DefaultRequestHeaders.Add("grantCode", grantCode);
            }
            //Make request
            try
            {
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
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
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
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
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
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
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


        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return null;

            //Create client
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }

            if (bearerToken != "")
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
            if (grantCode != "")
            {
                //client.DefaultRequestHeaders.Authorization. = new AuthenticationHeaderValue("grantCode", grantCode);
                client.DefaultRequestHeaders.Add("grantCode", grantCode);
            }

            //Make request
            try
            {
                return client.GetAsync(requestUri).Result;
            }
            catch
            {
                return null;
            }
        }
        public async Task<string> GetStringAsync(string requestUri)
        {
            //Validate
            if (requestUri == null || requestUri.Trim() == "") return default(string);

            //Create client
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }

            if (bearerToken != "")
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
            if (grantCode != "")
            {
                //client.DefaultRequestHeaders.Authorization. = new AuthenticationHeaderValue("grantCode", grantCode);
                client.DefaultRequestHeaders.Add("grantCode", grantCode);
            }

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
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }


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
            HttpClient client = new HttpClient();
            if (_httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                //client = _httpClientFactory.CreateClient(_clientName);
            }
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
