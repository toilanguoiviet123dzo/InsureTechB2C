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

        private async Task Load_SettingMaster()
        {
            try
            {
                SettingMasters.Clear();
                //
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

        /// <summary>
        /// Return setting record
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public async Task<SettingMasterModel> GetSetting(string Code)
        {
            //Get from DB
            if (SettingMasters.Count == 0)
            {
                await Load_SettingMaster();
            }
            //Get ret data
            return SettingMasters.Find(x => x.Code == Code);
        }
        /// <summary>
        /// Get string1 from settimg master
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public async Task<string> GetString1(string Code)
        {
            //Get from DB
            if (SettingMasters.Count == 0)
            {
                await Load_SettingMaster();
            }
            //Get ret data
            var setting = SettingMasters.Find(x => x.Code == Code);
            if (setting != null)
            {
                return setting.StringValue1;
            }
            //
            return "";
        }
        /// <summary>
        /// Get Int1 setting
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public async Task<int> GetInt1(string Code)
        {
            //Get from DB
            if (SettingMasters.Count == 0)
            {
                await Load_SettingMaster();
            }
            //Get ret data
            var setting = SettingMasters.Find(x => x.Code == Code);
            if (setting != null)
            {
                return setting.IntValue1;
            }
            //
            return 0;
        }
        /// <summary>
        /// Get double1
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public async Task<double> GetDouble1(string Code)
        {
            //Get from DB
            if (SettingMasters.Count == 0)
            {
                await Load_SettingMaster();
            }
            //Get ret data
            var setting = SettingMasters.Find(x => x.Code == Code);
            if (setting != null)
            {
                return setting.DoubleValue1;
            }
            //
            return 0.0;
        }



        //
    }
}
