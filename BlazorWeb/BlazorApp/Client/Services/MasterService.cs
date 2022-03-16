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
    public class MasterService
    {
        private readonly grpcAdminService.grpcAdminServiceClient _adminServiceClient;
        private readonly grpcInsureService.grpcInsureServiceClient _InsureServicesClient;
        
        public MasterService(grpcAdminService.grpcAdminServiceClient adminServiceClient,
                             grpcInsureService.grpcInsureServiceClient InsureServicesClient)
        {
            _adminServiceClient = adminServiceClient;
            _InsureServicesClient = InsureServicesClient;
        }
        //OptionLists
        private List<OptionListModel> OptionLists = new List<OptionListModel>();
        public async Task<List<OptionListModel>> Load_OptionList(string ListCode)
        {
            //Get from DB
            if (OptionLists.Count == 0)
            {
                try
                {
                    var request = new Admin.Services.String_Request()
                    {
                        Credential = new Admin.Services.UserCredential()
                        {
                            Username = WebUserCredential.Username,
                            RoleID = WebUserCredential.RoleID,
                            AccessToken = WebUserCredential.AccessToken,
                            ApiKey = WebUserCredential.ApiKey
                        },
                        StringValue = ""

                    };
                    //Get data from DB
                    var response = await _adminServiceClient.GetOptionListAsync(request);
                    if (response != null && response.ReturnCode == 200)
                    {
                        foreach (var item in response.OptionList)
                        {
                            var dataRow = new OptionListModel();
                            ClassHelper.CopyPropertiesDataDateConverted(item, dataRow);
                            OptionLists.Add(dataRow);
                        }
                    }
                    
                }
                catch { }
            }
            //Ger list by ListCode
            var result = new List<OptionListModel>();
            foreach (var record in OptionLists)
            {
                if (record.ListCode == ListCode)
                {
                    var dataRow = new OptionListModel();
                    ClassHelper.CopyPropertiesDataDateConverted(record, dataRow);
                    result.Add(dataRow);
                }
            }
            //Order
            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.DspOrder).ToList<OptionListModel>();
            }
            //
            return result;
        }

        //UserList
        private List<UserAccountModel> UserLists = new List<UserAccountModel>();
        public async Task<List<UserAccountModel>> Load_UserList()
        {
            try
            {
                if (UserLists.Count == 0)
                {
                    var request = new Admin.Services.String_Request();
                    request.Credential = new Admin.Services.UserCredential()
                    {
                        Username = WebUserCredential.Username,
                        RoleID = WebUserCredential.RoleID,
                        AccessToken = WebUserCredential.AccessToken,
                        ApiKey = WebUserCredential.ApiKey
                    };
                    //
                    var response = await _adminServiceClient.GetUserAccountAsync(request);
                    if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                    {
                        foreach (var item in response.UserAccounts)
                        {
                            //Parrent grid
                            var dataRow = new UserAccountModel();
                            ClassHelper.CopyPropertiesDataDateConverted(item, dataRow);
                            //
                            UserLists.Add(dataRow);
                        }
                    }
                    //Order
                    if (UserLists.Count > 0)
                    {
                        UserLists = UserLists.OrderBy(x => x.Fullname).ToList<UserAccountModel>();
                    }
                }
            }
            catch { }
            //
            return UserLists;
        }

        


    }
}
