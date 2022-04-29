using Cores.Utilities;
using Cores.Helpers;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;
using MongoDB.Entities;

namespace PaymentWeb.Services
{
    public class BHVService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpHelper _httpHelper;
        //
        public BHVService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpHelper = new HttpHelper(MyConstant.HttpClient_Common, _httpClientFactory);
        }
        

        private async Task<BhvResponseModel> Create_Tomato(mdSaleOrder saleOrder)
        {
            var ret = new BhvResponseModel();
            //
            try
            {
                //Get settings
                string createUrl = await SettingMaster.GetString1("021");
                if (string.IsNullOrEmpty(createUrl))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "SettingMaster", "021", ReturnCode.Error_ByServer, "021:Not setup - Create tomato Url");
                    return ret;
                }
                string accessToken = await SettingMaster.GetString1("022");
                if (string.IsNullOrEmpty(accessToken))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "SettingMaster", "022", ReturnCode.Error_ByServer, "021:Not setup - Access token");
                    return ret;
                }

                //Request
                var request = new BhvRequestModel();
                request.action_name = "tomato";
                request.token_access = accessToken;
                request.data = Create_Tomata_RequestBody();
                request.data_key = "";

                //Header
                var headers = new Dictionary<string, string>();
                headers.Add("grantCode", "xxx");

                //Motocycle
                if (saleOrder.ProductType == MyConstant.ProductType_Motocycle)
                {
                    ret = await _httpHelper.PostAsync<BhvRequestModel, BhvResponseModel>(createUrl, request, "", "", headers);
                }

               
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "CreateToIssue", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

        private string Create_Tomata_RequestBody()
        {
            string bodyData = "";
            //
            try
            {

            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        
       
        
        
    }


    class BhvRequestModel
    {
        public string action_name = "";
        public string token_access = "";
        public string data = "";
        public string data_key = "";
    }

    class BhvDataModel
    {
        public string collation_code = "";
        public string data_value = "";
    }

    class TomatoRequestDataModel
    {
        public string buyer_fullname = "";
        public string buyer_phone = "";
        public string buyer_birthday = "";
        public string buyer_email = "";
        public string buyer_job = "";
        public string buyer_identity_card = "";
        public string buyer_address = "";
        public string buyer_city = "";
        public string buyer_district = "";
        public string gc_customer_id = "";
        public string payment_premium = "";
    }

    class TomatoResponseDataModel
    {
        public string register_status = "";
        public string certificate_code = "";
        public string collation_code = "";
        public string benefit = "";
        public string premium_payment = "";
        public string active_date = "";
        public string inactive_date = "";
        public string created_date = "";
    }

    class BhvResponseModel
    {
        public string status_code = "";
        public string data = "";
        public string data_key = "";
    }



}
