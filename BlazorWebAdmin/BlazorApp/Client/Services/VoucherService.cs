using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Services;
using Insure.Services;
using Cores.GrpcClient.Authentication;
using BlazorApp.Client.BindingModels;
using Cores.Helpers;
using Cores.Grpc.Client;

namespace BlazorApp.Client.Services
{
    public class VoucherService
    {
        private readonly grpcAdminService.grpcAdminServiceClient _adminServiceClient;
        public VoucherService(grpcAdminService.grpcAdminServiceClient adminServiceClient)
        {
            _adminServiceClient = adminServiceClient;
        }

        public async Task<string> Get_NewVoucherNo(string voucherCode)
        {
            try
            {
                var request = new Admin.Services.String_Request();
                request.Credential = new Admin.Services.UserCredential()
                {
                    Username = WebUserCredential.Username,
                    RoleID = WebUserCredential.RoleID,
                    AccessToken = WebUserCredential.AccessToken,
                    ApiKey = WebUserCredential.ApiKey
                };
                request.StringValue = voucherCode;
                //
                var response = await _adminServiceClient.GetVoucherNoAsync(request);
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    return response.StringValue;
                }
            }
            catch { }
            //
            return "";
        }


    }
}
