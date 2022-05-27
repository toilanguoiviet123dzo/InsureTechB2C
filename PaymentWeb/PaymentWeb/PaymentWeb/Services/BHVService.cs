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

        private Dictionary<int, string> CarSeat { get; set; } = new Dictionary<int, string>()
        {
            {1,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5448" },
            {2,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5449" },
            {3,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5450" },
            {4,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5451" },
            {5,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5452" },
            {6,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5453" },
            {7,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5454" },
            {8,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5455" },
            {9,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5456" },
            {10,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5457" },
            {11,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5458" },
            {12,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5459" },
            {13,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5460" },
            {14,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5461" },
            {15,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5462" },
            {16,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5463" },
            {17,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5464" },
            {18,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5465" },
            {19,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5466" },
            {20,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5467" },
            {21,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5468" },
            {22,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5469" },
            {23,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5470" },
            {24,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5471" },
            {25,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5472" },
            {26,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5473" },
            {27,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5474" },
            {28,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5475" },
            {29,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5476" },
            {30,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5477" },
            {31,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5478" },
            {32,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5479" },
            {33,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5480" },
            {34,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5481" },
            {35,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5482" },
            {36,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5483" },
            {37,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5484" },
            {38,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5485" },
            {39,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5486" },
            {40,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5487" },
            {41,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5488" },
            {42,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5489" },
            {43,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5490" },
            {44,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5491" },
            {45,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5492" },
            {46,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5493" },
            {47,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5494" },
            {48,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5495" },
            {49,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5496" },
            {50,"33d227b2-dbb9-4fdf-9cf7-b0fa843d5497" }
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
                        var data = MyJson.Deserialize<AutoTNDS_CreateReturn>(response.data);
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
                                    }
                                    else
                                    {
                                        saleOrder.ProcessErrorMessage = "Không download được giấy chứng nhận";
                                    }

                                    //Success
                                    paymentSuccess = true;
                                    saleOrder.IsProcessDone = true;
                                    saleOrder.OrderID = data.data_value.certificate_code;
                                    saleOrder.PolicyNo = data.data_value.certificate_code;
                                }
                                else
                                {
                                    saleOrder.ProcessErrorMessage = ret.ErrorMessage;
                                }
                            }
                            else
                            {
                                saleOrder.ProcessErrorMessage = "Data trờ về rỗng";
                            }
                        }
                        else
                        {
                            saleOrder.ProcessErrorMessage = "TransactionID trả về không khớp";
                        }
                    }
                    else
                    {
                        saleOrder.ProcessErrorMessage = response.status_code;
                        //Get error
                        var errorData = MyJson.Deserialize<BhvResponseError>(response.data);
                        if (errorData != null)
                        {
                            saleOrder.ProcessErrorMessage += errorData.data_value + "_" + errorData.data_message;
                        }
                    }
                }
                else
                {
                    saleOrder.ProcessErrorMessage = "Call api with null return";
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_MotorTNDS", "Issue", ReturnCode.Error_ByServer, saleOrder.ProcessErrorMessage);
                MyAppLog.WriteLog(MyConstant.LogLevel_ForSale, "BHVService", "Create_MotorTNDS", "Issue", ReturnCode.Error_ByServer, logMessage);
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
                        var data = MyJson.Deserialize<MotorTNDS_PaymentReturn>(response.data);
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
                        var data = MyJson.Deserialize<MotorTNDS_DownloadReturn>(response.data);
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
                data.data_value = MyJson.Serialize(requestData);

                //Return
                return MyJson.Serialize(data);
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
                data.data_value = MyJson.Serialize(payData);

                //Return
                return MyJson.Serialize(data);
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
                data.data_value = MyJson.Serialize(reqData);

                //Return
                return MyJson.Serialize(data);
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
        private async Task<string> GetPrice_AutoTNDS(mdSaleOrder saleOrder)
        {
            var ret = "";
            //
            try
            {
                //Init
                if (!(await Init()))
                {
                    return ret;
                }

                //Request
                var request = new BhvRequest();
                request.action_name = "external/vehicle/car/load/premium";
                request.token_access = _accessToken;
                request.data = GetPrice_AutoTNDS_Data(saleOrder);
                request.data_key = await Sign_Data(request.data);

                //Motocycle
                var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                if (response != null && response.status_code == "200")
                {
                    var data = MyJson.Deserialize<AutoTNDS_GetPriceReturn>(response.data);
                    if (data != null && data.data_value != null)
                    {
                        return data.data_value[0].payment_value;
                    }
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Download_AutoTNDS_Certificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return ret;
        }

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
                request.action_name = "external/vehicle/car/register";
                request.token_access = _accessToken;
                request.data = await Create_AutoTNDS_Data(saleOrder);
                //Check data
                if (string.IsNullOrWhiteSpace(request.data))
                {
                    saleOrder.ProcessErrorMessage = "Không lấy được giá bán từ BHV";
                }

                request.data_key = await Sign_Data(request.data);

                //Motocycle
                if (!string.IsNullOrWhiteSpace(request.data))
                {
                    var response = await _httpHelper.PostAsync<BhvRequest, BhvResponse>(_url, request);
                    if (response != null)
                    {
                        if (response.status_code == "200")
                        {
                            //Update order
                            var data = MyJson.Deserialize<AutoTNDS_CreateReturn>(response.data);
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
                                        else
                                        {
                                            saleOrder.ProcessErrorMessage = "Không download được giấy chứng nhận";
                                        }

                                        //Success
                                        paymentSuccess = true;
                                        saleOrder.IsProcessDone = true;
                                        saleOrder.OrderID = data.data_value.certificate_code;
                                        saleOrder.PolicyNo = data.data_value.certificate_code;
                                    }
                                    else
                                    {
                                        saleOrder.ProcessErrorMessage = ret.ErrorMessage;
                                    }
                                }
                                else
                                {
                                    saleOrder.ProcessErrorMessage = "Data trờ về rỗng";
                                }
                            }
                            else
                            {
                                saleOrder.ProcessErrorMessage = "TransactionID trả về không khớp";
                            }
                        }
                        else
                        {
                            saleOrder.ProcessErrorMessage = response.status_code;
                            //Get error
                            var errorData = MyJson.Deserialize<BhvResponseError>(response.data);
                            if (errorData != null)
                            {
                                saleOrder.ProcessErrorMessage += errorData.data_value + "_" + errorData.data_message;
                            }
                        }
                    }
                    else
                    {
                        saleOrder.ProcessErrorMessage = "Call api with null return";
                    }
                }
                //
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_AutoTNDS", "Issue", ReturnCode.Error_ByServer, saleOrder.ProcessErrorMessage);
                MyAppLog.WriteLog(MyConstant.LogLevel_ForSale, "BHVService", "Create_AutoTNDS", "Issue", ReturnCode.Error_ByServer, logMessage);
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
                        var data = MyJson.Deserialize<AutoTNDS_PaymentReturn>(response.data);
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
                        var data = MyJson.Deserialize<AutoTNDS_DownloadReturn>(response.data);
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
                request.action_name = "external/vehicle/car/load/model";
                request.token_access = _accessToken;
                request.data = Common_AutoTNDS_Data(collation_code, "b07cadc0-b84d-4863-b374-fe018b3194c155");
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



        private async Task<string> Create_AutoTNDS_Data(mdSaleOrder saleOrder)
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
                requestData.CAR_NUMBER_PLATE = saleOrder.LicensePlate;
                requestData.CAR_CHASSIS = "";
                requestData.CAR_NUMBER_ENGINE = "";
                requestData.CAR_AUTOMAKER = "b07cadc0-b84d-4863-b374-fe018b3194c155";   //Toyota
                requestData.CAR_MODEL = "f07080e9-c23f-449f-83e9-f8c2a8778d0042";       //4Runner
                requestData.CAR_KIND = saleOrder.CarType;
                requestData.CAR_GOAL = saleOrder.BusinessType;
                requestData.CAR_SEAT = Get_SeatCode(saleOrder.SeatCount);
                requestData.CAR_WEIGH_GOODS = Get_TonageCode(saleOrder.Tonage);
                requestData.CAR_YEAR_BUY = "53d91be0-a641-4309-bd6b-e76befbe4e70";
                requestData.CAR_PACKAGE = "";
                requestData.BUY_POV = "0";
                requestData.BUY_CHK_CCL = "1";

                requestData.total_premium = await GetPrice_AutoTNDS(saleOrder);
                if (string.IsNullOrWhiteSpace(requestData.total_premium))
                {
                    return "";
                }

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = saleOrder.TransactionID;
                data.data_value = MyJson.Serialize(requestData);

                //Return
                return MyJson.Serialize(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        private string GetPrice_AutoTNDS_Data(mdSaleOrder saleOrder)
        {
            string bodyData = "";
            //
            try
            {
                //AutoTNDSRequestData
                var requestData = new AutoTNDS_GetPriceData();
                requestData.CAR_AUTOMAKER = "b07cadc0-b84d-4863-b374-fe018b3194c155";   //Toyota
                requestData.CAR_MODEL = "f07080e9-c23f-449f-83e9-f8c2a8778d0042";       //4Runner
                requestData.CAR_KIND = saleOrder.CarType;
                requestData.CAR_GOAL = saleOrder.BusinessType;
                requestData.CAR_SEAT = Get_SeatCode(saleOrder.SeatCount);
                requestData.CAR_WEIGH_GOODS = Get_TonageCode(saleOrder.Tonage);
                requestData.CAR_YEAR_BUY = "53d91be0-a641-4309-bd6b-e76befbe4e70";
                requestData.CAR_PACKAGE = "";
                requestData.BUY_POV = "0";
                requestData.BUY_CHK_CCL = "1";

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = saleOrder.TransactionID;
                data.data_value = MyJson.Serialize(requestData);

                //Return
                return MyJson.Serialize(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        private string Get_SeatCode(double seatCount)
        {
            if (CarSeat.ContainsKey((int)seatCount))
            {
                return CarSeat[(int)seatCount];
            }
            //
            return "";
        }
        private string Get_TonageCode(double tonage)
        {
            if (tonage == 0) return "";
            //Dưới 3 tấn
            if (tonage < 3.0) return "297966cd-be47-4d7e-a52c-9a1297ca8009";
            //Từ 3 đến 8 tấn
            if (tonage >= 3.0 && tonage <= 8.0) return "297966cd-be47-4d7e-a52c-9a1297ca8010";
            //Trên 8 đến 15 tấn
            if (tonage > 8.0 && tonage <= 15.0) return "297966cd-be47-4d7e-a52c-9a1297ca8011";
            //Trên 15 tấn
            if (tonage > 15.0) return "297966cd-be47-4d7e-a52c-9a1297ca8009";
            //
            return "";
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
                data.data_value = MyJson.Serialize(payData);

                //Return
                return MyJson.Serialize(data);
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
                data.data_value = MyJson.Serialize(reqData);

                //Return
                return MyJson.Serialize(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Create_TPLForm", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return bodyData;
        }

        private string Common_AutoTNDS_Data(string collation_code, string carMaker)
        {
            string bodyData = "";
            //
            try
            {
                //Car category
                var carCategory = new AutoTNDS_CarCategory();
                carCategory.root_id = carMaker;

                //BhvDataModel
                var data = new BhvRequestData();
                data.collation_code = collation_code;
                data.data_value = MyJson.Serialize(carCategory);

                //Return
                return MyJson.Serialize(data);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "BHVService", "Common_AutoTNDS_Data", "Exception", ReturnCode.Error_ByServer, ex.Message);
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

    class BhvResponseError
    {
        public string collation_code = "";
        public string data_value = "";
        public string data_type = "";
        public string data_message = "";
        public string data_message_type = "";
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
        public string total_premium = "";
        public string CAR_NUMBER_PLATE = "";
        public string CAR_CHASSIS = "";
        public string CAR_NUMBER_ENGINE = "";
        public string CAR_AUTOMAKER = "";
        public string CAR_MODEL = "";
        public string CAR_KIND = "";
        public string CAR_GOAL = "";
        public string CAR_SEAT = "";
        public string CAR_WEIGH_GOODS = "";
        public string CAR_YEAR_BUY = "";
        public string CAR_PACKAGE = "";
        public string BUY_POV = "0";
        public string BUY_CHK_CCL = "1";
    }

    class AutoTNDS_GetPriceData
    {
        public string CAR_AUTOMAKER = "";
        public string CAR_MODEL = "";
        public string CAR_KIND = "";
        public string CAR_GOAL = "";
        public string CAR_SEAT = "";
        public string CAR_WEIGH_GOODS = "";
        public string CAR_YEAR_BUY = "";
        public string CAR_PACKAGE = "";
        public string BUY_POV = "0";
        public string BUY_CHK_CCL = "1";
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

    class AutoTNDS_GetPriceReturn
    {
        public string collation_code = "";
        public List<AutoTNDS_GetPriceReturnData> data_value = new List<AutoTNDS_GetPriceReturnData>();
    }

    class AutoTNDS_GetPriceReturnData
    {
        public string benefit_name = "";
        public string payment_value = "";
        public string tax_value = "";
        public string discount = "";
        public string seat_buy = "";
        public string year_buy = "";
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

    class AutoTNDS_CarCategory
    {
        public string root_id = "";
    }





}
