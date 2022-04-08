using Cores.Utilities;
using Cores.Helpers;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;

namespace PaymentWeb.Services
{
    public class eBaoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpHelper _httpHelper;
        private string AccessToken = "";
        private string GrantCode = "";
        private int ExpireTime = 0;
        public eBaoService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpHelper = new HttpHelper(MyConstant.HttpClient_Common, _httpClientFactory);
        }

        private async Task GetAccess()
        {
            try
            {
                //Check timeout
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;
                if (ExpireTime > secondsSinceEpoch)
                {
                    return;
                }

                //Get Access token
                string ticketUsername = await SettingMaster.GetString1("010");
                string ticketPassword = await SettingMaster.GetString1("011");
                AccessToken = await GetAccessToken(new eBaoUserModel() { username = ticketUsername, password = ticketPassword });

                //Get grantcode
                string grancodeUsername = await SettingMaster.GetString1("012");
                string grancodePassword = await SettingMaster.GetString1("013");
                GrantCode = await GetGrandCode(new eBaoUserModel() { username = grancodeUsername, password = grancodePassword });
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "GetAccess", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
        }

        private async Task<string> GetAccessToken(eBaoUserModel ebaoUser)
        {
            try
            {
                string ticketUrl = await SettingMaster.GetString1("008");
                if (string.IsNullOrEmpty(ticketUrl))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "SettingMaster", "008", ReturnCode.Error_ByServer, "008: not setup");
                    return "";
                }
                //
                var reqData = new Dictionary<string, string>();
                reqData.Add("username", ebaoUser.username);
                reqData.Add("password", ebaoUser.password);
                //
                var response = await _httpHelper.PostAsNewtonsoftJsonAsync<Dictionary<string, string>, TokenResponseModel>(ticketUrl, reqData);
                if (response != null)
                {
                    TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    int secondsSinceEpoch = (int)t.TotalSeconds;
                    //
                    ExpireTime = secondsSinceEpoch + response.expire_in - 100;
                    return response.access_token;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "GetToken", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return "";
        }

        private async Task<string> GetGrandCode(eBaoUserModel ebaoUser)
        {
            try
            {
                string grantCodeUrl = await SettingMaster.GetString1("009");
                if (string.IsNullOrEmpty(grantCodeUrl))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "SettingMaster", "009", ReturnCode.Error_ByServer, "009: not setup");
                    return "";
                }
                var reqData = new Dictionary<string, string>();
                reqData.Add("username", ebaoUser.username);
                reqData.Add("password", ebaoUser.password);
                //
                var response = await _httpHelper.PostAsNewtonsoftJsonAsync<Dictionary<string, string>, GrandCodeResponseModel>(grantCodeUrl, reqData, AccessToken, "Bearer");
                if (response != null && response.success)
                {
                    return response.data;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "GetGrandCode", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            return "";
        }

        public async Task<IssueResponse> CreateToIssue(mdSaleOrder saleOrder)
        {
            var ret = new IssueResponse();
            //
            try
            {
                //Get settings
                string createToIssueUrl = await SettingMaster.GetString1("014");
                if (string.IsNullOrEmpty(createToIssueUrl))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "SettingMaster", "014", ReturnCode.Error_ByServer, "014: not setup");
                    return ret;
                }

                //Get Access
                await GetAccess();

                //Request data
                var newForm = new TPLForm();
                newForm.insurerTenantCode = "BM_VN";
                newForm.proposalDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.effDate = saleOrder.EffectiveSttDate.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.expDate = saleOrder.EffectiveEndDate.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.prdtCode = "TPL";
                newForm.referenceNo = saleOrder.OrderID;
                newForm.insurerTenantCode = "BM_VN";
                //insureds
                var veh = new InsuredModel();
                veh.detail.vehicleChassisNo = saleOrder.LicensePlate;
                veh.detail.vehicleEngineNo = "";
                veh.detail.vehicleRegNo = "";
                newForm.insureds.Add(veh);

                //policyholder
                var policyholder = new PolicyHolderModel();
                policyholder.customer = new CustomerModel();
                policyholder.customer.idNo = saleOrder.CusCitizenID;
                policyholder.customer.firstName = saleOrder.CusFullname.GetFirstName();
                policyholder.customer.lastName = saleOrder.CusFullname.GetLastName();
                policyholder.customer.nationality = "VNM";
                policyholder.customer.mobile = saleOrder.CusPhone;
                policyholder.customer.email = saleOrder.CusEmail;
                policyholder.customer.address = new AddressModel();
                policyholder.customer.address.province = saleOrder.CityID;
                policyholder.customer.address.district = saleOrder.DistrictID;
                policyholder.customer.address.subDistrict = saleOrder.WardID;
                policyholder.customer.address.postalCode = saleOrder.PostalCode;
                policyholder.customer.address.addressLine1 = saleOrder.Address;
                newForm.policyholder = policyholder;

                //payer
                var payer = new PayerModel();
                payer.customer = policyholder.customer;
                newForm.payer = payer;

                //deliveryInfo
                var deliveryInfo = new DeliveryInfoModel();
                deliveryInfo.firstName = policyholder.customer.firstName;
                deliveryInfo.lastName = policyholder.customer.lastName;
                deliveryInfo.extInfo = new ContactModel();
                deliveryInfo.extInfo.contactNo = saleOrder.CusPhone;
                newForm.deliveryInfo = deliveryInfo;
                //
                //Call issue
                var headers = new Dictionary<string, string>();
                headers.Add("grantCode", GrantCode);
                //
                return await _httpHelper.PostAsNewtonsoftJsonAsync<TPLForm, IssueResponse>(createToIssueUrl, newForm, AccessToken, "Bearer", headers);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "CreateToIssue", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

        public async Task<string> GetCertificateLink(string policyID)
        {
            try
            {
                string certificateLink = "";

                //Get settings
                string printFileListUrl = await SettingMaster.GetString1("015");
                if (string.IsNullOrEmpty(printFileListUrl))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "SettingMaster", "015", ReturnCode.Error_ByServer, "015: not setup");
                    return "";
                }
                string downLoadFileUrl = await SettingMaster.GetString1("016");
                if (string.IsNullOrEmpty(downLoadFileUrl))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "SettingMaster", "016", ReturnCode.Error_ByServer, "016: not setup");
                    return "";
                }

                //Get print file list
                //Call issue
                var headers = new Dictionary<string, string>();
                headers.Add("grantCode", GrantCode);
                //
                var response = await _httpHelper.GetAsNewtonsoftJsonAsync<PrintListModel>($"{printFileListUrl}/{policyID}", AccessToken, "Bearer", headers);
                if (response != null && response.success && response.data != null)
                {
                    foreach (var item in response.data)
                    {
                        certificateLink = item.prdtCode;
                    }

                    //Get download link
                    var requestData = new Dictionary<string, string>();
                    requestData.Add("policyId", policyID);
                    requestData.Add("prdtCode", response.data[0].prdtCode);
                    requestData.Add("printFileType", response.data[0].printFileTypes[0]);
                    var resDownload = await _httpHelper.PostAsNewtonsoftJsonAsync<Dictionary<string, string>, FileDownloadModel>(downLoadFileUrl, requestData, AccessToken, "Bearer", headers);
                    if (resDownload != null)
                    {
                        return resDownload.redirectUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "GetCertificateLink", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            return "";
        }
        //
    }

    class FileDownloadModel
    {
        public string redirectUrl { get; set; } = "";
        public string redirectMethod { get; set; } = "";
        public string redirectParams { get; set; } = "";
        public string fileFullName { get; set; } = "";
        public string fileSubName { get; set; } = "";
    }

    class TokenResponseModel
    {
        public string access_token = "";
        public int expire_in;
    }

    class eBaoUserModel
    {
        public string username = "";
        public string password = "";
    }

    class GrandCodeResponseModel
    {
        public bool success;
        public string data = "";
    }

    public class DataModel
    {
        public string processStatus = "";
        public string processMsg = "";
        public string policyId = "";
        public string quoteNo = "";
        public string policyNo = "";
        public string paymentRedirectURL = "";
        public List<string> subPolicyResults = new List<string>();
    }
    public class IssueResponse
    {
        public DataModel data = new DataModel();
    }

    class ExtInfoModel
    {
        public string infoType = "Motorcycle";
        public int vehicleType = 6;
        public bool coverPA = false;
    }
    class DetailModel
    {
        public int insuredType = 2;
        public ExtInfoModel extInfo = new ExtInfoModel();
        public int vehicleId = 2;
        public string vehicleCountry = "VNM";
        public string vehicleProvince = "10";
        public string vehicleChassisNo = "296948";
        public string vehicleRegNo = "REGNO32333";
        public string vehicleEngineNo = "ENGINENO3351";
        public int vehicleRegYear = 2020;
        public bool isNewVehicle = false;
        public int capacity = 51;
    }
    class InsuredModel
    {
        public DetailModel detail = new DetailModel();
    }
    class PayModeModel
    {
        public string payMode = "credit";
        public ExtInfoModel extInfo = new ExtInfoModel();
    }
    class ContactModel
    {
        public string contactNo = "0783500797";
        public int sendTo = 1;
    }
    class DeliveryInfoModel
    {
        public int deliveryMethod = 1;
        public string firstName = "Mike";
        public string lastName = "T";
        public ContactModel extInfo = new ContactModel();
    }
    class CustomerModel
    {
        public int customerType = 1;
        public PreferredLangModel extInfo = new PreferredLangModel();

        public int idType = 5;
        public string idNo = "98567464";
        public int prefix = 6;
        public string firstName = "Mike";
        public string lastName = "T";
        public string nationality = "THA";
        public string mobile = "0783500797";
        public string email = "mike@ebao.com";
        public string occupation = "1001";
        public string taxNo = "";
        public string branch = "";
        public AddressModel address = new AddressModel();

    }
    public class PreferredLangModel
    {
        public int preferredLang = 2;
    }
    public class AddressModel
    {
        public int addressType = 1;
        public string province = "10";
        public string district = "084";
        public string subDistrict = "02815";
        public string postalCode = "02815";
        public string addressLine1 = "strng2";
    }
    class PayerModel
    {
        public int payerType = 2;
        public CustomerModel customer = new CustomerModel();
    }
    class PolicyHolderModel
    {
        public bool isSameAsInsured = false;
        public CustomerModel customer = new CustomerModel();
    }
    class TPLForm
    {
        public string insurerTenantCode = "BM_VN";
        public string proposalDate = "15/04/2022 16:30:00";
        public string effDate = "15/04/2022 16:30:00";
        public string expDate = "15/04/2023 16:30:00";
        public string prdtCode = "TPL";
        public string referenceNo = "Test0254";
        public List<InsuredModel> insureds = new List<InsuredModel>();

        public PolicyHolderModel policyholder = new PolicyHolderModel();
        public PayerModel payer = new PayerModel();
        public DeliveryInfoModel deliveryInfo = new DeliveryInfoModel();
        public List<string> documents = new List<string>();
        public PayModeModel payMode = new PayModeModel();
        public ExtInfoModel extInfo = new ExtInfoModel();

    }

    class PrintListModel
    {
        public bool success;
        public string errorCode = "";
        public List<DataPrintListModel> data = new List<DataPrintListModel>();
    }
    public class DataPrintListModel
    {
        public string prdtCode = "";
        public List<string> printFileTypes = new List<string>();
    }
}
