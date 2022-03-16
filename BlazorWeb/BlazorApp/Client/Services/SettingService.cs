using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Services;
using Cores.GrpcClient.Authentication;
using Cores.Grpc.Client;
using BlazorApp.Client.BindingModels;
using Cores.Helpers;

namespace BlazorApp.Client.Services
{
    public class SettingService
    {
        private readonly grpcAdminService.grpcAdminServiceClient _adminServiceClient;
        private List<SettingMasterModel> SettingMasters = new List<SettingMasterModel>();
        public SettingService(grpcAdminService.grpcAdminServiceClient adminServiceClient)
        {
            _adminServiceClient = adminServiceClient;
        }

        public async Task<SettingMasterModel> GetSetting(string Code)
        {
            //Get from DB
            if (SettingMasters.Count == 0)
            {
                try
                {
                    var request = new Empty_Request()
                    {
                        Credential = new UserCredential()
                        {
                            Username = WebUserCredential.Username,
                            RoleID = WebUserCredential.RoleID,
                            AccessToken = WebUserCredential.AccessToken,
                            ApiKey = WebUserCredential.ApiKey
                        }
                    };
                    //Call grpc
                    var response = await _adminServiceClient.GetSettingMasterAsync(request);
                    if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                    {
                        foreach (var item in response.Records)
                        {
                            var row = new SettingMasterModel();
                            ClassHelper.CopyPropertiesData(item, row);
                            SettingMasters.Add(row);
                        }
                    }
                }
                catch { }
            }
            //Get ret data
            return SettingMasters.Find(x => x.Code == Code);
        }
    }
}
