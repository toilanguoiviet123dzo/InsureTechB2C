using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using Insurtech.Common;
using System.IO;

namespace eBaoIssuanceAPI
{

    class Program
    {


        static HttpClient client = new HttpClient();
        static string token = "";
        static string grantCode = "";

        static void Main(string[] args)
        {
            
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch
            {

            }

        }


        static async Task RunAsync()
        {
            //Get token
            await GetToken(new UserPass() { username = "ddpc.admin", password = "eBaosg1234" });
            Thread.Sleep(1000);

            //Get GrantCode
            await GetGrandCode(new UserPass() { username = "itb_sales", password = "eBao1234" });

            //It is recommended to refectch token and get grant code regularly (token has expiration)



            var newForm = new TPLForm();
            //To-do: Input form info
            var veh = new InsuredModel();
            var randomGenerator = new Random();
            randomGenerator.Next(1, 1000000);
            newForm.referenceNo = "Test" + randomGenerator.Next(1, 1000000).ToString();
            veh.detail.vehicleChassisNo = "";
            veh.detail.vehicleEngineNo = "";
            veh.detail.vehicleRegNo = "555555555";
            newForm.insureds.Add(veh);
            ////////////////////////

            var response = await CreateToIssue(newForm);
            try
            {
                if (response.data.policyId != "")
                    PrintCertificate(response.data.policyId);
            }
            catch
            {

            }
        }

        static async Task<TokenResponse> GetToken(UserPass userPass)
        {
            var x = new Dictionary<string, string>();
            x.Add("username", userPass.username);
            x.Add("password", userPass.password);
            var _httpHelper = new HttpHelper();
            var httpResponse = await _httpHelper.PostAsNewtonsoftJsonAsync<Dictionary<string, string>, TokenResponse>("https://sandbox.sg.ebaocloud.com/cas/ebao/v1/json/tickets", x);
            token = httpResponse.access_token;
            return httpResponse;
        }

        static async Task<GrandCodeResponse> GetGrandCode(UserPass userPass)
        {
            var x = new Dictionary<string, string>();
            x.Add("username", userPass.username);
            x.Add("password", userPass.password);
            var _httpHelper = new HttpHelper();
            _httpHelper.SetToken(token);
            var httpResponse = await _httpHelper.PostAsNewtonsoftJsonAsync<Dictionary<string, string>, GrandCodeResponse>("https://sandbox.gw.sg.ebaocloud.com/ddpc/1.0.0/api/pub/std/utils/grantCode", x);
            grantCode = httpResponse.data;
            return httpResponse;
        }

        static async Task<IssueResponse> CreateToIssue<T>(T form)
        {
            //var x = new Dictionary<string, string>();
            
            var _httpHelper = new HttpHelper();
            _httpHelper.SetToken(token);
            _httpHelper.SetGrantCode(grantCode);
            var httpResponse = await _httpHelper.PostAsNewtonsoftJsonAsync<T, IssueResponse>("https://sandbox.gw.sg.ebaocloud.com/ddpc/1.0.0/api/pub/std/quotation/createToIssue", form);
            
            return httpResponse;
        }



        //Use this method to trigger BM sms
        static async void PrintCertificate(string policyId)
        {
            //Calling Get infos for downloading files
            var _httpHelper = new HttpHelper();
            _httpHelper.SetToken(token);
            _httpHelper.SetGrantCode(grantCode);
            var httpResponse = await _httpHelper.GetAsync("https://sandbox.gw.sg.ebaocloud.com/ddpc/1.0.0/api/pub/std/policy/printFile/list/" + policyId);
            string responseBody = await httpResponse.Content.ReadAsStringAsync();
            var respondObj = JsonConvert.DeserializeObject<PrintListModel>(responseBody);


            //Calling download
            //var dat = new DataDownloadModel();
            var dat = new Dictionary<string, string>();
            try
            {
                dat.Add("policyId", policyId);
                dat.Add("prdtCode", respondObj.data[0].prdtCode);
                dat.Add("printFileType", respondObj.data[0].printFileTypes[0]);
            }
            catch
            {

            }
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("grantCode", grantCode);
            string json = JsonConvert.SerializeObject(dat, Formatting.Indented);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var downloadObj = client.PostAsync("https://sandbox.gw.sg.ebaocloud.com/ddpc/1.0.0/api/pub/std/policy/printFile/download", content).Result;
            responseBody = await downloadObj.Content.ReadAsStringAsync();
            respondObj = JsonConvert.DeserializeObject<PrintListModel>(responseBody);
            Console.WriteLine(responseBody);

        }

    }
}
