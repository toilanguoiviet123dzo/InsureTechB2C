using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Headers;
using Admin.Services;
using Cores.Grpc.Client;
using Blazored.LocalStorage;

namespace Cores.GrpcClient.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _autStateProvider;
        private readonly grpcAdminService.grpcAdminServiceClient _adminServiceClient;

        public AuthenticationService(AuthenticationStateProvider autStateProvider,
                                     ILocalStorageService localStorage,
                                     grpcAdminService.grpcAdminServiceClient adminServiceClient)
        {
            _localStorage = localStorage;
            _autStateProvider = autStateProvider;
            _adminServiceClient = adminServiceClient;
        }

        public async Task<bool> Login(AuthenticationRequestModel userForAuthentication)
        {
            //Clear
            var loginResult = false;
            WebUserCredential.Username = "";
            WebUserCredential.Fullname = "";
            WebUserCredential.RoleID = "";
            //
            try
            {
                //Login from local
                string userName = await _localStorage.GetItemAsync<string>("Username");
                string fuleName = await _localStorage.GetItemAsync<string>("Fullname");
                string roleID = await _localStorage.GetItemAsync<string>("RoleID");
                string merchanID = await _localStorage.GetItemAsync<string>("MerchantID");
                string accessToken = await _localStorage.GetItemAsync<string>("AccessToken");

                //
                var newAccessToken = await Check_AccessToken(userName, roleID, accessToken);
                if (!string.IsNullOrWhiteSpace(newAccessToken))
                {
                    //OK
                    WebUserCredential.Username = userName;
                    WebUserCredential.Fullname = fuleName;
                    WebUserCredential.RoleID = roleID;
                    WebUserCredential.MerchantID = merchanID;
                    WebUserCredential.AccessToken = newAccessToken;
                    WebUserCredential.IsAuthenticated = true;
                    //
                    //Notify login
                    ((AuthStateProvider)_autStateProvider).NotifyUserAuthentication();

                    //
                    return true;
                }


                //gRpc login
                var request = new GrpcLogin_Request()
                {
                    Credential = new UserCredential()
                    {
                        Username = WebUserCredential.Username,
                        RoleID = WebUserCredential.RoleID,
                        AccessToken = WebUserCredential.AccessToken,
                        ApiKey = WebUserCredential.ApiKey
                    },
                    UserName = userForAuthentication.UserName,
                    Password = userForAuthentication.Password
                };

                //Call grpc
                var response = await _adminServiceClient.GrpcLoginAsync(request);
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    WebUserCredential.Username = response.UserName;
                    WebUserCredential.Fullname = response.Fullname;
                    WebUserCredential.RoleID = response.RoleID;
                    WebUserCredential.MerchantID = response.MerchantID;
                    WebUserCredential.AccessToken = response.AccessToken;
                    //
                    loginResult = true;

                    //Save to local storage
                    await _localStorage.SetItemAsync("Username", WebUserCredential.Username);
                    await _localStorage.SetItemAsync("Fullname", WebUserCredential.Fullname);
                    await _localStorage.SetItemAsync("RoleID", WebUserCredential.RoleID);
                    await _localStorage.SetItemAsync("MerchantID", WebUserCredential.MerchantID);
                    await _localStorage.SetItemAsync("AccessToken", WebUserCredential.AccessToken);

                    //Notify login
                    ((AuthStateProvider)_autStateProvider).NotifyUserAuthentication();
                }
            }
            catch { }
            //
            return loginResult;
        }

        public void Development_Login()
        {
            //Clear
            WebUserCredential.Username = "AdminDev";
            WebUserCredential.Fullname = "Admin Development";
            WebUserCredential.RoleID = "99";
            WebUserCredential.MerchantID = "InsurTech";
            WebUserCredential.IsDevelopmentMode = true;
            //
            //Notify login
            ((AuthStateProvider)_autStateProvider).NotifyUserAuthentication();
        }

        public async void Logout()
        {
            //Notify logout
            ((AuthStateProvider)_autStateProvider).NotifyUserLogout();

            //Remove in local storage
            await _localStorage.RemoveItemAsync("Username");
            await _localStorage.RemoveItemAsync("Fullname");
            await _localStorage.RemoveItemAsync("RoleID");
            await _localStorage.RemoveItemAsync("AccessToken");
            await _localStorage.RemoveItemAsync("MerchantID");
        }

        public async Task<string> Check_AccessToken(string userName, string roleID, string accessToken)
        {
            try
            {
                //validate
                if (string.IsNullOrWhiteSpace(userName)) return "";
                if (string.IsNullOrWhiteSpace(roleID)) return "";
                if (string.IsNullOrWhiteSpace(accessToken)) return "";

                //gRpc login
                var request = new CheckAccessToken_Request()
                {
                    Credential = new Admin.Services.UserCredential()
                    {
                        Username = WebUserCredential.Username,
                        RoleID = WebUserCredential.RoleID,
                        AccessToken = WebUserCredential.AccessToken,
                        ApiKey = WebUserCredential.ApiKey
                    },
                    UserName = userName,
                    RoleID = roleID,
                    AccessToken = accessToken
                };
                //Call grpc
                var response = await _adminServiceClient.CheckAccessTokenAsync(request);
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    return response.StringValue;
                }
            }
            catch { }
            //NG
            return "";
        }


    }
}
