using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cores.Grpc.Authentication
{
    public static class AuthenticationService
    {
        public static readonly string EncryptKey = "1234567890!@#$%^&*()5963";
        public static Dictionary<string, string> ApiKeyDic;

        public static void Init()
        {
            ApiKeyDic = new Dictionary<string, string>();

            //Get authentication apiKey list
            //return await GrpcClientFactory.CallServiceAsync<GetUser_Response>(ServiceList.User, async channel =>
            //{
            //    var client = new grpcAccount.grpcAccountClient(channel);
            //    return await client.GetUserAsync(request);
            //});

            //string apiKey = CryptHelper.Encrypt(EncryptKey, hashedApiKey);

            ApiKeyDic.Add("apiKey5963", "adminUser");

        }

        public static bool CheckApiKey(string hashedApiKey)
        {
            if (ApiKeyDic != null)
            {
                return ApiKeyDic.ContainsKey(hashedApiKey);
            }
            return true;
        }


    }
}
