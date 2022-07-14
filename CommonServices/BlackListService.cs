using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gosu.Auth.Services;
using Gosu.Common;
using static Gosu.Grpc.Authentication.AuthInterceptor;

namespace Gosu.Grpc.Authentication
{
    //kha add
    public class ClientAppDictItem
    {
        public string ClientID { get; set; }
        public string ClientSignature { get; set; }
        public string SecretKey { get; set; }
        public int Enable { get; set; }
    }
    public class Response
    {
        public int ReturnCode { get; set; }
        public bool BoolValue { get; set; }
        public string ErrorMessage { get; set; } = "";


    }
    //
    public static class BlackListService
    {
        private static List<string> _blacklist = new List<string>();
        private static Dictionary<string, ClientAppDictItem> ClientApps = new Dictionary<string, ClientAppDictItem>();

        public static async Task<bool> Check_BlackList(string ClientID)
        {
            if (_blacklist.Count == 0)
            {
                await Load_BlackList();
            }
            //Check
            return _blacklist.Contains(ClientID);
        }

        private static async Task Load_BlackList()
        {
            //Clear
            _blacklist.Clear();

            //Load from DB
            try
            {
                var request = new GetClientApp_Request();
                request.Credential = new Auth.Services.UserCredential() { ApiKey = GrpcCredential.ApiKey };
                //
                var response = await GrpcClientFactory.CallServiceAsync<Auth.Services.GetClientApp_Response>(ServiceList.Auth, async channel =>
                {
                    var client = new grpcAuthService.grpcAuthServiceClient(channel);
                    return await client.GetClientAppAsync(request);
                });
                //
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    //
                    foreach (var item in response.ClientApps)
                    {
                        if (item.Enabled == 0)
                        {
                            _blacklist.Add(item.ClientID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BlackListService", "Load_BlackList", "1", 500, ex.Message);
            }
        }
        //
        public static async Task<Response> Valid_Appclient(string ClientID, string Params, string Signature, string Method)
        {
            var res = new Response();
            res.ReturnCode = 200;
            res.BoolValue = true;
            res.ErrorMessage = "";
            string sign = "";
            string notCheckSignMethodList = ConfigurationHelper.NotCheckSignList();
            try
            {
                if (notCheckSignMethodList.Contains(Method))
                {
                    Params = "Exist";
                }
                if (string.IsNullOrEmpty(Params))
                {
                    res.ReturnCode = 301;
                    res.BoolValue = false;
                    res.ErrorMessage = "missing parameter";
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BlackListService", Method, "1", res.ReturnCode, res.ErrorMessage);
                    return res;
                }
                var request = new GetClientApp_Request();
                request.ClientID = ClientID;
                request.Credential = new Auth.Services.UserCredential() { ApiKey = GrpcCredential.ApiKey };
                //
                var response = await GrpcClientFactory.CallServiceAsync<Auth.Services.GetClientApp_Response>(ServiceList.Auth, async channel =>
                {
                    var client = new grpcAuthService.grpcAuthServiceClient(channel);
                    return await client.GetClientAppAsync(request);
                });
                //
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    //
                    var ClientAppDictItem = new ClientAppDictItem();
                    foreach (var item in response.ClientApps)
                    {
                        ClientAppDictItem.ClientID = item.ClientID;
                        ClientAppDictItem.ClientSignature = item.ClientSignature;
                        ClientAppDictItem.SecretKey = item.SecretKey;
                        ClientAppDictItem.Enable = item.Enabled;
                    }
                    if (ClientAppDictItem.Enable == 0)
                    {
                        res.ReturnCode = 302;
                        res.BoolValue = false;
                        res.ErrorMessage = "clientID unabled";
                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BlackListService", Method, "1", res.ReturnCode, res.ErrorMessage);
                        return res;
                    }
                    //
                    sign = (ClientID + Params + ClientAppDictItem.SecretKey).Md5();
                    if (notCheckSignMethodList.Contains(Method))
                    {
                        Signature = sign;
                    }
                    if (Signature != sign)
                    {
                        res.ReturnCode = 303;
                        res.BoolValue = false;
                        res.ErrorMessage = "wrong sign";
                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BlackListService", Method, "1", res.ReturnCode, res.ErrorMessage);
                        return res;
                    }
                }
                else
                {
                    res.ReturnCode = 304;
                    res.BoolValue = false;
                    res.ErrorMessage = "system error";
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BlackListService", Method, "1", res.ReturnCode, res.ErrorMessage);
                    return res;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BlackListService", Method, "1", 500, ex.Message);
                res.ReturnCode = 500;
                res.BoolValue = false;
                return res;
            }            
            return res;
        }
    }
}
