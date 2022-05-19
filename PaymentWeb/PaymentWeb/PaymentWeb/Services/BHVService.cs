using Cores.Utilities;
using Cores.Helpers;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;
using MongoDB.Entities;
using Newtonsoft.Json;

namespace PaymentWeb.Services
{
    public class BHVService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpHelper _httpHelper;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string _url = "";
        private string _accessToken = "";
        private string _certificateFolder = "";
        //
        public BHVService(IHttpClientFactory httpClientFactory,
                          IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpClientFactory = httpClientFactory;
            _httpHelper = new HttpHelper(MyConstant.HttpClient_Common, _httpClientFactory);
        }

        #region Common functions

        private Dictionary<string, string> ErrorDic { get; set; } = new Dictionary<string, string>()
        {
            {"EST1","Lỗi hệ thống vui lòng liên hệ Admin" },
            {"EST2","Dữ liệu đầu vào không đúng theo cấu trúc json" },
            {"EST3","Dữ liệu data không đúng cấu trúc" },
            {"EST4","collation_code không hợp lệ" },
            {"EST5","Không tồn tại action_name" },
            {"EST6","token_access không hợp lệ" },
            {"EST7","Không tìm thấy thông tin kết nối (URL)" },
            {"EST8","data_key không hợp lệ" },
            {"EST9","lỗi giải mã dữ liệu" },
            {"EST10","chữ ký dữ liệu không hợp lệ" },
            {"EST11","tài khoản không có quyền truy cập chức năng" },
            {"EST12","Không có quyền truy cập chức năng" },
            {"EX2B26","Chưa chọn tỉnh/ thành phố" },
            {"EX2B12","Phải nhập biển số xe hoặc số khung + số máy" },
            {"EX2B11","Vui lòng xem lại phí" },
            {"EX2B25","Số điện thoại không hợp lệ, độ dài tối thiểu là 10 số" },
            {"EX2B23","Chưa nhập email hoặc chưa đúng định dạng email" },
            {"EX2B24","Chưa nhập chứng minh nhân dân/ thẻ căn cước" },
            {"EX2B2","Không tạo được giấy chứng nhận" },
            {"EX2B1","Không tìm thấy thông tin hợp đồng" }
        };

        private async Task<bool> Init()
        {
            try
            {
                //skip check
                if (!string.IsNullOrWhiteSpace(_url)) return true;

                //Url
                _url = await SettingMaster.GetString1("021");
                if (string.IsNullOrEmpty(_url))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "SettingMaster", "021", ReturnCode.Error_ByServer, "021:Not setup - Create mortor Url");
                    return false;
                }
                //_accessToken
                _accessToken = await SettingMaster.GetString1("022");
                if (string.IsNullOrEmpty(_accessToken))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "SettingMaster", "022", ReturnCode.Error_ByServer, "021:Not setup - Access token");
                    return false;
                }
                //_certificateFolder
                _certificateFolder = await SettingMaster.GetString1("024");
                if (string.IsNullOrEmpty(_certificateFolder))
                {
                    _certificateFolder = "BHVCertificates";
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "SettingMaster", "024", ReturnCode.Error_ByServer, "024:Not setup - BHV: Certificate folder");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Init", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return true;
        }

        public async Task<string> Sign_Data(string message)
        {
            string signedData = "";
            try
            {
                //Get settings
                string privateKey = await SettingMaster.GetString1("023");
                if (string.IsNullOrEmpty(privateKey))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "SettingMaster", "023", ReturnCode.Error_ByServer, "023:Not setup - BHV: private key");
                    return signedData;
                }

                //sign
                return RSACrypto.SignData_SHA256(message, privateKey);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Sign_Data", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            return signedData;
        }

        private bool WritePdf_Certificate(string certificate_code, string certificate_base64content)
        {
            try
            {
                string filename = @$"{_hostingEnvironment.WebRootPath}/{_certificateFolder}/{certificate_code}.pdf";
                //Create folder
                string folder = @$"{_hostingEnvironment.WebRootPath}/{_certificateFolder}";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                byte[] bytes = Convert.FromBase64String(certificate_base64content);
                MyFile.Write_ToBinary(filename, bytes);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "WritePdf_Certificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
                return false;
            }
            return true;
        }
        #endregion

        #region MotorTNDS
        public async Task<CallApiReturn> Create_MotorTNDS(mdSaleOrder saleOrder)
        {
            var ret = new CallApiReturn();
            bool paymentSuccess = false;
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }

                //Request
                var request = new BhvRequest();
                request.action_name = "external/vehicle/motor/register";
                request.token_access = _accessToken;
                request.data = Create_MotorTNDS_Data(saleOrder);
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        //Update order
                        var data = JsonConvert.DeserializeObject<MotorTNDS_CreateReturn>(response.data);
                        if (data != null && data.collation_code == saleOrder.TransactionID)
                        {
                            if (data.data_value != null)
                            {
                                //Call payment
                                ret = await Payment_MotorTNDS(data.collation_code, data.data_value.certificate_code);
                                if (ret.ReturnCode == ReturnCode.OK)
                                {
                                    //Download certificate
                                    saleOrder.IsIssueCertificate = false;
                                    ret = await Download_MotorTNDS(data.collation_code, data.data_value.certificate_code);
                                    if (ret.ReturnCode == ReturnCode.OK)
                                    {
                                        saleOrder.IsIssueCertificate = true;
                                        saleOrder.CertificateLink = @$"{MyData.BaseUrl}/{_certificateFolder}/{data.data_value.certificate_code}.pdf";

                                        //Success
                                        paymentSuccess = true;
                                        saleOrder.IsProcessDone = true;
                                        saleOrder.OrderID = data.data_value.certificate_code;
                                        saleOrder.PolicyNo = data.data_value.certificate_code;
                                    }
                                    else
                                    {
                                        saleOrder.ProcessErrorMessage = ret.ErrorMessage;
                                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_MotorTNDS", "Exception", ret.ReturnCode, ret.ErrorMessage);
                                    }
                                }
                                else
                                {
                                    saleOrder.ProcessErrorMessage = ret.ErrorMessage;
                                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_MotorTNDS", "Exception", ret.ReturnCode, ret.ErrorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = response.status_code;
                        saleOrder.ProcessErrorMessage = errorMessage;
                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_MotorTNDS", "Exception", ReturnCode.Error_ByServer, errorMessage);
                    }
                }
                else
                {
                    saleOrder.ProcessErrorMessage = "Call api with null return";
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_MotorTNDS", "Exception", ReturnCode.Error_ByServer, saleOrder.ProcessErrorMessage);
                }
            }
            catch (Exception ex)
            {
                saleOrder.ProcessErrorMessage = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "CreateToIssue", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //Payment error
            if (!paymentSuccess)
            {
                saleOrder.IsProcessError = true;
                ret.ReturnCode = ReturnCode.Error_203;
                ret.ErrorMessage = MyMessage.Error_PaymenmtError;
                //Payment error log
                string logMessage = "Đã nhận tiền khách hàng nhưng không cấp được đơn" + Environment.NewLine;
                logMessage += $"Tên khách hàng: {saleOrder.CusFullname}" + Environment.NewLine;
                logMessage += $"Điện thoại: {saleOrder.CusPhone}" + Environment.NewLine;
                logMessage += $"Mua sản phẩm: {saleOrder.ProductName}" + Environment.NewLine;
                logMessage += $"Giá tiền: {saleOrder.UnitPrice}" + Environment.NewLine;
                logMessage += $"Giảm giá: {saleOrder.DiscountAmount}" + Environment.NewLine;
                logMessage += $"Tổng thanh toán: {saleOrder.PaymentAmount}" + Environment.NewLine;
                //
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Payment", "Exception", ReturnCode.Error_ByServer, logMessage);
            }
            //Save
            await saleOrder.SaveAsync();
            //
            return ret;
        }

        private async Task<CallApiReturn> Payment_MotorTNDS(string collation_code, string certificate_code)
        {
            var ret = new CallApiReturn();
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }

                //Request
                var request = new BhvRequest();
                request.action_name = "external/payment/complete";
                request.token_access = _accessToken;
                request.data = Payment_MotorTNDS_Data(collation_code, certificate_code);
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        var data = JsonConvert.DeserializeObject<MotorTNDS_PaymentReturn>(response.data);
                        if (data != null && data.collation_code == collation_code)
                        {
                            if (data.data_value == null || data.data_value.certificate_code != certificate_code)
                            {
                                ret.ReturnCode = ReturnCode.Error_202;
                                ret.ErrorMessage = "Mã chứng nhận trả lại không đúng từ BHV";
                            }
                        }
                    }
                    else
                    {
                        ret.ReturnCode = ReturnCode.Error_202;
                        ret.ErrorMessage = ErrorDic[response.status_code];
                    }
                }
                else
                {
                    ret.ReturnCode = ReturnCode.Error_ByServer;
                    ret.ErrorMessage = MyMessage.Error_ServerError;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Payment_MotorTNDS", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

        private async Task<CallApiReturn> Download_MotorTNDS(string collation_code, string certificate_code)
        {
            var ret = new CallApiReturn();
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }


                //Request
                var request = new BhvRequest();
                request.action_name = "external/render/certificate";
                request.token_access = _accessToken;
                request.data = Download_MotorTNDS_Data(collation_code, certificate_code);
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        var data = JsonConvert.DeserializeObject<MotorTNDS_DownloadReturn>(response.data);
                        if (data != null && data.collation_code == collation_code)
                        {
                            if (data.data_value != null)
                            {
                                if (!WritePdf_Certificate(certificate_code, data.data_value))
                                {
                                    ret.ReturnCode = ReturnCode.Error_202;
                                    ret.ErrorMessage = "Lưu giấy chứng nhận thất bại";
                                }
                            }
                        }
                    }
                    else
                    {
                        ret.ReturnCode = ReturnCode.Error_202;
                        ret.ErrorMessage = ErrorDic[response.status_code];
                    }
                }
                else
                {
                    ret.ReturnCode = ReturnCode.Error_ByServer;
                    ret.ErrorMessage = MyMessage.Error_ServerError;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Download_MotorTNDS_Certificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

        private async Task<CallApiReturn> GetCommonData_MotorTNDS(string collation_code, string certificate_code)
        {
            var ret = new CallApiReturn();
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }

                //Request
                var request = new BhvRequest();
                request.action_name = "external/load/district";
                request.token_access = _accessToken;
                request.data = Download_MotorTNDS_Data(collation_code, "eee52932-0358-47d5-9984-be1132c7f79");
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        var data = response.data;
                    }
                    else
                    {
                        ret.ReturnCode = ReturnCode.Error_202;
                        ret.ErrorMessage = ErrorDic[response.status_code];
                    }
                }
                else
                {
                    ret.ReturnCode = ReturnCode.Error_ByServer;
                    ret.ErrorMessage = MyMessage.Error_ServerError;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Download_MotorTNDS_Certificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }



        private string Create_MotorTNDS_Data(mdSaleOrder saleOrder)
        {
            string bodyData = "";
            //
            try
            {
                //MotorTNDSRequestData
                var requestData = new MotorTNDS_CreateData();
                requestData.buyer_fullname = saleOrder.CusFullname;
                requestData.buyer_phone = saleOrder.CusPhone;
                requestData.buyer_email = saleOrder.CusEmail;
                requestData.buyer_identity_card = saleOrder.CusCitizenID;
                requestData.buyer_address = "68 Đường số 1, phường Tân Phú";
                requestData.buyer_city = "eee52932-0358-47d5-9984-be1132c7f79";
                requestData.buyer_district = "6c24e5c0-1791-4c09-9810-00b80102778";
                requestData.active_date = saleOrder.EffectiveEndDate.ToString("dd/MM/yyyy");
                requestData.total_payment = saleOrder.PaymentAmount.ToString();
                requestData.MOTOR_NUMBER_PLATE = saleOrder.LicensePlate;
                requestData.MOTOR_CHASSIS = "";
                requestData.MOTOR_NUMBER_ENGINE = "";
                requestData.MOTOR_TYPE = "a58419fd-6a1e-4409-9a5f-eb9ff90ab417";
                requestData.MOTOR_YEAR_BUY = "e7b66343-62ca-4064-881c-2b39f4bd33ab";
                requestData.MOTOR_PACKAGE = "";
                requestData.CHK_BUY_MOTOR_CCL = "1";

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = saleOrder.TransactionID;
                data.data_value = JsonConvert.SerializeObject(requestData);

                //Return
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        private string Payment_MotorTNDS_Data(string collation_code, string certificate_code)
        {
            string bodyData = "";
            //
            try
            {
                //Payment
                var payData = new MotorTNDS_Payment();
                payData.certificate_code = certificate_code;

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = collation_code;
                data.data_value = JsonConvert.SerializeObject(payData);

                //Return
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        private string Download_MotorTNDS_Data(string collation_code, string certificate_code)
        {
            string bodyData = "";
            //
            try
            {
                //Data
                var reqData = new MotorTNDS_Download();
                reqData.certificate_code = certificate_code;

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = collation_code;
                data.data_value = JsonConvert.SerializeObject(reqData);

                //Return
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }
        #endregion


        #region AutoTNDS
        public async Task<CallApiReturn> Create_AutoTNDS(mdSaleOrder saleOrder)
        {
            var ret = new CallApiReturn();
            bool paymentSuccess = false;
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }

                //@@@ test
                //await GetCommonData_AutoTNDS(saleOrder.TransactionID, "");

                //Request

                var request = new BhvRequest();
                request.action_name = "external/vehicle/motor/register";
                request.token_access = _accessToken;
                request.data = Create_AutoTNDS_Data(saleOrder);
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        //Update order
                        var data = JsonConvert.DeserializeObject<AutoTNDS_CreateReturn>(response.data);
                        if (data != null && data.collation_code == saleOrder.TransactionID)
                        {
                            if (data.data_value != null)
                            {
                                //Call payment
                                ret = await Payment_AutoTNDS(data.collation_code, data.data_value.certificate_code);
                                if (ret.ReturnCode == ReturnCode.OK)
                                {
                                    //Download certificate
                                    saleOrder.IsIssueCertificate = false;
                                    ret = await Download_AutoTNDS(data.collation_code, data.data_value.certificate_code);
                                    if (ret.ReturnCode == ReturnCode.OK)
                                    {
                                        saleOrder.IsIssueCertificate = true;
                                        saleOrder.CertificateLink = @$"{MyData.BaseUrl}/{_certificateFolder}/{data.data_value.certificate_code}.pdf";
                                    }

                                    //Success
                                    paymentSuccess = true;
                                    saleOrder.IsProcessDone = true;
                                    saleOrder.OrderID = data.data_value.certificate_code;
                                    saleOrder.PolicyNo = data.data_value.certificate_code;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "CreateToIssue", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //Payment error
            if (!paymentSuccess)
            {
                saleOrder.IsProcessError = true;
                ret.ReturnCode = ReturnCode.Error_203;
                ret.ErrorMessage = MyMessage.Error_PaymenmtError;
                //Payment error log
                string logMessage = "Đã nhận tiền khách hàng nhưng không cấp được đơn" + Environment.NewLine;
                logMessage += $"Tên khách hàng: {saleOrder.CusFullname}";
                logMessage += $"Điện thoại: {saleOrder.CusPhone}";
                logMessage += $"Mua sản phẩm: {saleOrder.ProductName}";
                logMessage += $"Giá tiền: {saleOrder.UnitPrice}";
                logMessage += $"Giảm giá: {saleOrder.DiscountAmount}";
                logMessage += $"Tổng thanh toán: {saleOrder.PaymentAmount}";
                //
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Payment", "Exception", ReturnCode.Error_ByServer, logMessage);
            }
            //Save
            await saleOrder.SaveAsync();
            //
            return ret;
        }

        private async Task<CallApiReturn> Payment_AutoTNDS(string collation_code, string certificate_code)
        {
            var ret = new CallApiReturn();
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }

                //Request
                var request = new BhvRequest();
                request.action_name = "external/payment/complete";
                request.token_access = _accessToken;
                request.data = Payment_AutoTNDS_Data(collation_code, certificate_code);
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        var data = JsonConvert.DeserializeObject<AutoTNDS_PaymentReturn>(response.data);
                        if (data != null && data.collation_code == collation_code)
                        {
                            if (data.data_value == null || data.data_value.certificate_code != certificate_code)
                            {
                                ret.ReturnCode = ReturnCode.Error_202;
                                ret.ErrorMessage = "Mã chứng nhận trả lại không đúng từ BHV";
                            }
                        }
                    }
                    else
                    {
                        ret.ReturnCode = ReturnCode.Error_202;
                        ret.ErrorMessage = ErrorDic[response.status_code];
                    }
                }
                else
                {
                    ret.ReturnCode = ReturnCode.Error_ByServer;
                    ret.ErrorMessage = MyMessage.Error_ServerError;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Payment_AutoTNDS", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

        private async Task<CallApiReturn> Download_AutoTNDS(string collation_code, string certificate_code)
        {
            var ret = new CallApiReturn();
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }

                //Request
                var request = new BhvRequest();
                request.action_name = "external/render/certificate";
                request.token_access = _accessToken;
                request.data = Download_AutoTNDS_Data(collation_code, certificate_code);
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        var data = JsonConvert.DeserializeObject<AutoTNDS_DownloadReturn>(response.data);
                        if (data != null && data.collation_code == collation_code)
                        {
                            if (data.data_value != null)
                            {
                                if (!WritePdf_Certificate(certificate_code, data.data_value))
                                {
                                    ret.ReturnCode = ReturnCode.Error_202;
                                    ret.ErrorMessage = "Lưu giấy chứng nhận thất bại";
                                }
                            }
                        }
                    }
                    else
                    {
                        ret.ReturnCode = ReturnCode.Error_202;
                        ret.ErrorMessage = ErrorDic[response.status_code];
                    }
                }
                else
                {
                    ret.ReturnCode = ReturnCode.Error_ByServer;
                    ret.ErrorMessage = MyMessage.Error_ServerError;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Download_AutoTNDS_Certificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

        private async Task<CallApiReturn> GetCommonData_AutoTNDS(string collation_code, string certificate_code)
        {
            var ret = new CallApiReturn();
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Init failed";
                    return ret;
                }

                //Request
                var request = new BhvRequest();
                request.action_name = "external/load/district";
                request.token_access = _accessToken;
                request.data = Download_AutoTNDS_Data(collation_code, "eee52932-0358-47d5-9984-be1132c7f79");
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null)
                {
                    if (response.status_code == "200")
                    {
                        var data = response.data;
                    }
                    else
                    {
                        ret.ReturnCode = ReturnCode.Error_202;
                        ret.ErrorMessage = ErrorDic[response.status_code];
                    }
                }
                else
                {
                    ret.ReturnCode = ReturnCode.Error_ByServer;
                    ret.ErrorMessage = MyMessage.Error_ServerError;
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Download_AutoTNDS_Certificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }



        private string Create_AutoTNDS_Data(mdSaleOrder saleOrder)
        {
            string bodyData = "";
            //
            try
            {
                //AutoTNDSRequestData
                var requestData = new AutoTNDS_CreateData();
                requestData.buyer_fullname = saleOrder.OwnerFullname;
                requestData.buyer_phone = saleOrder.CusPhone;
                requestData.buyer_email = saleOrder.CusEmail;
                requestData.buyer_identity_card = saleOrder.CusCitizenID;
                requestData.buyer_address = "68 Đường số 1, phường Tân Phú";
                requestData.buyer_city = "eee52932-0358-47d5-9984-be1132c7f79";
                requestData.buyer_district = "6c24e5c0-1791-4c09-9810-00b80102778";
                requestData.active_date = saleOrder.EffectiveEndDate.ToString("dd/MM/yyyy");
                requestData.total_payment = saleOrder.PaymentAmount.ToString();
                requestData.MOTOR_NUMBER_PLATE = saleOrder.LicensePlate;
                requestData.MOTOR_CHASSIS = "";
                requestData.MOTOR_NUMBER_ENGINE = "";
                requestData.MOTOR_TYPE = "a58419fd-6a1e-4409-9a5f-eb9ff90ab417";
                requestData.MOTOR_YEAR_BUY = "e7b66343-62ca-4064-881c-2b39f4bd33ab";
                requestData.MOTOR_PACKAGE = "";
                requestData.CHK_BUY_MOTOR_CCL = "1";

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = saleOrder.TransactionID;
                data.data_value = JsonConvert.SerializeObject(requestData);

                //Return
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        private string Payment_AutoTNDS_Data(string collation_code, string certificate_code)
        {
            string bodyData = "";
            //
            try
            {
                //Payment
                var payData = new AutoTNDS_Payment();
                payData.certificate_code = certificate_code;

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = collation_code;
                data.data_value = JsonConvert.SerializeObject(payData);

                //Return
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        private string Download_AutoTNDS_Data(string collation_code, string certificate_code)
        {
            string bodyData = "";
            //
            try
            {
                //Data
                var reqData = new AutoTNDS_Download();
                reqData.certificate_code = certificate_code;

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = collation_code;
                data.data_value = JsonConvert.SerializeObject(reqData);

                //Return
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }
        #endregion


    }


    class BhvRequest
    {
        public string action_name = "";
        public string token_access = "";
        public string data = "";
        public string data_key = "";
    }

    class BhvRequestData
    {
        public string collation_code = "";
        public string data_value = "";
    }

    class BhvResponse
    {
        public string status_code = "";
        public string data = "";
        public string data_key = "";
    }


    class MotorTNDS_DownloadReturn
    {
        public string collation_code = "";
        public string data_value = "";
    }

    class MotorTNDS_CreateData
    {
        public string buyer_fullname = "";
        public string buyer_phone = "";
        public string buyer_email = "";
        public string buyer_identity_card = "";
        public string buyer_address = "";
        public string buyer_city = "";
        public string buyer_district = "";
        public string active_date = "";
        public string total_payment = "";
        public string MOTOR_NUMBER_PLATE = "";
        public string MOTOR_CHASSIS = "";
        public string MOTOR_NUMBER_ENGINE = "";
        public string MOTOR_TYPE = "";
        public string MOTOR_YEAR_BUY = "1";
        public string MOTOR_PACKAGE = "";
        public string CHK_BUY_MOTOR_CCL = "1";
        public string CHK_BUY_MOTOR_VPA = "0";
    }

    class MotorTNDS_CreateReturn
    {
        public string collation_code = "";
        public MotorTNDS_CreateReturnData? data_value;
    }

    class MotorTNDS_CreateReturnData
    {
        public string certificate_code = "";
        public string buyer_fullname = "";
        public string buyer_phone = "";
        public string buyer_email = "";
        public string premium_payment = "";
        public string active_date = "";
        public string inactive_date = "";
        public string is_active = "";
    }

    class MotorTNDS_Payment
    {
        public string certificate_code = "";
    }
    class MotorTNDS_PaymentReturn
    {
        public string collation_code = "";
        public MotorTNDS_PaymentReturnData? data_value;
    }
    class MotorTNDS_PaymentReturnData
    {
        public string certificate_code = "";
        public string active_date = "";
        public string inactive_date = "";
        public string premium_payment = "";
    }

    class MotorTNDS_Download
    {
        public string certificate_code = "";
    }

    class AutoTNDS_DownloadReturn
    {
        public string collation_code = "";
        public string data_value = "";
    }

    class AutoTNDS_CreateData
    {
        public string buyer_fullname = "";
        public string buyer_phone = "";
        public string buyer_email = "";
        public string buyer_identity_card = "";
        public string buyer_address = "";
        public string buyer_city = "";
        public string buyer_district = "";
        public string active_date = "";
        public string total_payment = "";
        public string MOTOR_NUMBER_PLATE = "";
        public string MOTOR_CHASSIS = "";
        public string MOTOR_NUMBER_ENGINE = "";
        public string MOTOR_TYPE = "";
        public string MOTOR_YEAR_BUY = "1";
        public string MOTOR_PACKAGE = "";
        public string CHK_BUY_MOTOR_CCL = "1";
        public string CHK_BUY_MOTOR_VPA = "0";
    }

    class AutoTNDS_CreateReturn
    {
        public string collation_code = "";
        public AutoTNDS_CreateReturnData? data_value;
    }

    class AutoTNDS_CreateReturnData
    {
        public string certificate_code = "";
        public string buyer_fullname = "";
        public string buyer_phone = "";
        public string buyer_email = "";
        public string premium_payment = "";
        public string active_date = "";
        public string inactive_date = "";
        public string is_active = "";
    }

    class AutoTNDS_Payment
    {
        public string certificate_code = "";
    }
    class AutoTNDS_PaymentReturn
    {
        public string collation_code = "";
        public AutoTNDS_PaymentReturnData? data_value;
    }
    class AutoTNDS_PaymentReturnData
    {
        public string certificate_code = "";
        public string active_date = "";
        public string inactive_date = "";
        public string premium_payment = "";
    }

    class AutoTNDS_Download
    {
        public string certificate_code = "";
    }





}
