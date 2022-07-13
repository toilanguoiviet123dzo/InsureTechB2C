using Cores.Utilities;
using Cores.Helpers;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;
using MongoDB.Entities;

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

        private Dictionary<string, int> CarTypeDic { get; set; } = new Dictionary<string, int>()
        {
            {"11",1},    //Xe chở người
            {"12",10},    //Xe tải
            {"13",2},    //Xe vừa chở người vừa chở hàng (pickup/minivan)
            {"110",11},    //Xe tập lái
            {"16",12},    //Xe đầu kéo
            {"19",13},    //Xe chuyên dụng
            {"21",5},    //Xe chở người
            {"22",4},    //Xe tải
            {"23",3},    //Xe vừa chở người vừa chở hàng (pickup/minivan)
            {"24",14},    //Xe taxi
            {"25",15},    //Xe buýt
            {"26",16},    //Xe đầu kéo rơ moóc
            {"27",17},    //Xe cứu thương
            {"28",18},    //Xe chở tiền
            {"29",19}    //Xe chuyên dụng
        };

        private async Task GetAccess()
        {
            try
            {
                //Check timeout
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;
                if (!string.IsNullOrWhiteSpace(GrantCode) && !string.IsNullOrWhiteSpace(AccessToken) && ExpireTime > secondsSinceEpoch)
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

        private async Task<IssueResponse> CreateToIssue(mdSaleOrder saleOrder)
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

                //Header
                var headers = new Dictionary<string, string>();
                headers.Add("grantCode", GrantCode);

                //Motocycle
                if (saleOrder.ProductType == MyConstant.ProductType_Motocycle)
                {
                    var tplForm = Create_TPLForm(saleOrder);
                    ret = await _httpHelper.PostAsNewtonsoftJsonAsync<TPLForm, IssueResponse>(createToIssueUrl, tplForm, AccessToken, "Bearer", headers);
                }

                //AutoMotor
                if (saleOrder.ProductType == MyConstant.ProductType_AutoMotor)
                {
                    var tplForm = Create_CarTPLForm(saleOrder);
                    ret = await _httpHelper.PostAsNewtonsoftJsonAsync<CarTPLForm, IssueResponse>(createToIssueUrl, tplForm, AccessToken, "Bearer", headers);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "CreateToIssue", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

        private TPLForm Create_TPLForm(mdSaleOrder saleOrder)
        {
            var newForm = new TPLForm();
            //
            try
            {
                newForm.insurerTenantCode = "BM_VN";
                newForm.proposalDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.effDate = saleOrder.EffectiveSttDate.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.expDate = saleOrder.EffectiveEndDate.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.prdtCode = "TPL";
                newForm.referenceNo = saleOrder.TransactionID;
                newForm.insurerTenantCode = "BM_VN";
                //insureds
                var veh = new InsuredModel();
                //2 nguoi ngoi tren xe
                veh.detail.extInfo.coverPA = saleOrder.Motor2People;
                veh.detail.vehicleChassisNo = "";
                veh.detail.vehicleEngineNo = "";
                veh.detail.vehicleRegNo = saleOrder.LicensePlate;
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
                payer.customer = new PayerCustomerModel();
                ClassHelper.CopyPropertiesData(policyholder.customer, payer.customer);
                newForm.payer = payer;

                //deliveryInfo
                var deliveryInfo = new DeliveryInfoModel();
                deliveryInfo.firstName = policyholder.customer.firstName;
                deliveryInfo.lastName = policyholder.customer.lastName;
                deliveryInfo.extInfo = new ContactModel();
                deliveryInfo.extInfo.contactNo = saleOrder.CusPhone;
                deliveryInfo.address = new AddressModel();
                ClassHelper.CopyPropertiesData(policyholder.customer.address, deliveryInfo.address);
                newForm.deliveryInfo = deliveryInfo;
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return newForm;
        }

        private CarTPLForm Create_CarTPLForm(mdSaleOrder saleOrder)
        {
            var newForm = new CarTPLForm();
            //
            try
            {
                newForm.insurerTenantCode = "BM_VN";
                newForm.proposalDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.effDate = saleOrder.EffectiveSttDate.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.expDate = saleOrder.EffectiveEndDate.ToString("dd/MM/yyyy HH:mm:ss");
                newForm.prdtCode = "TPL";
                newForm.referenceNo = saleOrder.TransactionID;
                newForm.insurerTenantCode = "BM_VN";
                //insureds
                var veh = new CarInsuredModel();
                veh.detail.vehicleChassisNo = "";
                veh.detail.vehicleEngineNo = "";
                veh.detail.vehicleRegNo = saleOrder.LicensePlate;
                veh.detail.extInfo.vehicleCategory = int.Parse(saleOrder.BusinessType);
                //
                string carTypeKey = saleOrder.BusinessType + saleOrder.CarType;
                if (CarTypeDic.ContainsKey(carTypeKey))
                {
                    veh.detail.extInfo.vehicleType = CarTypeDic[carTypeKey];
                }
                veh.detail.numOfSeat = (int)saleOrder.SeatCount;
                veh.detail.tonnage = (int)saleOrder.Tonage;
                newForm.insureds.Add(veh);

                //policyholder
                var policyholder = new CarPolicyHolderModel();
                policyholder.customer = new CarCustomerModel();
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
                payer.customer = new PayerCustomerModel();
                ClassHelper.CopyPropertiesData(policyholder.customer, payer.customer);
                newForm.payer = payer;

                //deliveryInfo
                var deliveryInfo = new DeliveryInfoModel();
                deliveryInfo.firstName = policyholder.customer.firstName;
                deliveryInfo.lastName = policyholder.customer.lastName;
                deliveryInfo.extInfo = new ContactModel();
                deliveryInfo.extInfo.contactNo = saleOrder.CusPhone;
                deliveryInfo.address = new AddressModel();
                ClassHelper.CopyPropertiesData(policyholder.customer.address, deliveryInfo.address);
                newForm.deliveryInfo = deliveryInfo;
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return newForm;
        }

        private async Task<string> GetCertificateLink(string policyID)
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
        /// <summary>
        /// Issue eBao order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<CallApiReturn> eBao_CreateToIssue_TNDS(mdSaleOrder order)
        {
            var ret = new CallApiReturn();
            ret.ReturnCode = ReturnCode.OK;
            bool paymentSuccess = false;
            //
            try
            {
                var res = await CreateToIssue(order);
                if (res != null && res.data != null && res.data.processStatus == "PASS")
                {
                    //Success
                    paymentSuccess = true;
                    order.IsProcessDone = true;
                    order.PolicyID = res.data.policyId;
                    order.PolicyNo = res.data.policyNo;
                    order.QuoteNo = res.data.quoteNo;

                    //Download certificate
                    order.CertificateLink = await GetCertificateLink(order.PolicyID);
                }
                else
                {
                    //Failed
                    order.IsProcessError = true;
                    string errorMessage = "call api with null reponse";
                    if (res != null && res.data != null) errorMessage = res.data.processMsg;
                    order.ProcessErrorMessage = errorMessage;
                    //
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "CreateToIssue", "Failed", ReturnCode.Error_ByServer, errorMessage);
                }

            }
            catch (Exception ex)
            {
                order.ProcessErrorMessage = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "CreateToIssue", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //Payment error
            if (!paymentSuccess)
            {
                order.IsProcessError = true;
                ret.ReturnCode = ReturnCode.Error_203;
                ret.ErrorMessage = MyMessage.Error_PaymenmtError;
                //Payment error log
                string logMessage = "Đã nhận tiền khách hàng nhưng không cấp được đơn" + Environment.NewLine;
                logMessage += $"Tên khách hàng: {order.CusFullname}" + Environment.NewLine;
                logMessage += $"Điện thoại: {order.CusPhone}" + Environment.NewLine;
                logMessage += $"Mua sản phẩm: {order.ProductName}" + Environment.NewLine;
                logMessage += $"Giá tiền: {order.UnitPrice}" + Environment.NewLine;
                logMessage += $"Giảm giá: {order.DiscountAmount}" + Environment.NewLine;
                logMessage += $"Tổng thanh toán: {order.PaymentAmount}" + Environment.NewLine;
                //
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "eBaoService", "Issue", "Issue", ReturnCode.Error_ByServer, order.ProcessErrorMessage);
                MyAppLog.WriteLog(MyConstant.LogLevel_ForSale, "eBaoService", "Issue", "Issue", ReturnCode.Error_ByServer, logMessage);
            }
            //Save
            await order.SaveAsync();
            //
            return ret;
        }
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

    class CarExtInfoModel
    {
        public string infoType = "AutoMotor";
        public int vehicleCategory = 1;
        public int vehicleType = 1;
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

    class CarDetailModel
    {
        public int insuredType = 2;
        public CarExtInfoModel extInfo = new CarExtInfoModel();
        public int vehicleId = 2;
        public string vehicleCode = "1.10";
        public string vehicleCountry = "VNM";
        public string vehicleProvince = "10";
        public string vehicleChassisNo = "296948";
        public string vehicleRegNo = "REGNO32333";
        public string vehicleEngineNo = "ENGINENO3351";
        public int vehicleRegYear = 2020;
        public bool isNewVehicle = false;
        public int capacity = 0;
        public int numOfSeat = 0;
        public int tonnage = 0;
    }
    class InsuredModel
    {
        public DetailModel detail = new DetailModel();
    }

    class CarInsuredModel
    {
        public CarDetailModel detail = new CarDetailModel();
    }
    class PayModeModel
    {
        public string payMode = "credit";
        public object extInfo = new object();
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
        public AddressModel address = new AddressModel();
    }
    class CustomerModel
    {
        public int customerType = 1;
        public CusExtraInfo extInfo = new CusExtraInfo();
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

    class CarCustomerModel
    {
        public int customerType = 1;
        public CusExtraInfo extInfo = new CusExtraInfo();
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
        public string dob = "";
        public int age = 0;
        public AddressModel address = new AddressModel();
    }

    class PayerCustomerModel
    {
        public int customerType = 1;
        public PayerExtraInfo extInfo = new PayerExtraInfo();
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
    public class CusExtraInfo
    {
        public string phoneNo = "";
        public int preferredLang = 2;
    }

    public class PayerExtraInfo
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
        public PayerCustomerModel customer = new PayerCustomerModel();
    }
    class PolicyHolderModel
    {
        public bool isSameAsInsured = false;
        public CustomerModel customer = new CustomerModel();
    }

    class CarPolicyHolderModel
    {
        public bool isSameAsInsured = false;
        public CarCustomerModel customer = new CarCustomerModel();
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

    class CarTPLForm
    {
        public string insurerTenantCode = "BM_VN";
        public string proposalDate = "15/04/2022 16:30:00";
        public string effDate = "15/04/2022 16:30:00";
        public string expDate = "15/04/2023 16:30:00";
        public string prdtCode = "TPL";
        public string referenceNo = "Test0254";
        public List<CarInsuredModel> insureds = new List<CarInsuredModel>();

        public CarPolicyHolderModel policyholder = new CarPolicyHolderModel();
        public PayerModel payer = new PayerModel();
        public DeliveryInfoModel deliveryInfo = new DeliveryInfoModel();
        public List<string> documents = new List<string>();
        public PayModeModel payMode = new PayModeModel();
        public object extInfo = new object();

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
