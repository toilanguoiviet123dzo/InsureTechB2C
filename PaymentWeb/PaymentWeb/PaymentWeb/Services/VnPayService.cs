using BlazorApp.Server.Common;
using BlazorApp.Server.Models;
using Cores.Utilities;
using MongoDB.Entities;

namespace PaymentWeb.Services
{
    public class VnPayService
    {
        private eBaoService _ebaoService;
        private BHVService _bhvService;
        public VnPayService(eBaoService ebaoService,
                            BHVService bhvService)
        {
            _ebaoService = ebaoService;
            _bhvService = bhvService;
        }
        public static Dictionary<string, string> VnPay_ErrorDic { get; set; } = new Dictionary<string, string>()
        {
            {"00","Giao dịch thành công" },
            {"07","Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)." },
            {"09","Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng." },
            {"10","Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần" },
            {"11","Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch." },
            {"12","Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa." },
            {"13","Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch." },
            {"24","Giao dịch không thành công do: Khách hàng hủy giao dịch" },
            {"51","Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch." },
            {"65","Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày." },
            {"75","Ngân hàng thanh toán đang bảo trì." },
            {"79","Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch" },
            {"99","Thanh toán thất bại chưa rõ nguyên nhân" }
        };

        /// <summary>
        /// Init payment
        /// </summary>
        /// <param name="initOrderToken"></param>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public async Task<InitPaymentResult> InitPayment(string initOrderToken, string transactionID)
        {
            var ret = new InitPaymentResult();
            ret.ReturnCode = ReturnCode.OK;
            //
            try
            {
                //201: UnAuthorized
                //202: No transaction found
                //Check security
                if (!MyTokenService.Check_InitOrderToken(initOrderToken, transactionID))
                {
                    ret.ReturnCode = ReturnCode.Error_201;
                    return ret;
                }

                //Check Transaction
                var orderRecord = await DB.Find<mdSaleOrder>()
                                          .Match(x => x.TransactionID == transactionID)
                                          .Match(x => x.IsPayRequest == false)
                                          .ExecuteFirstAsync();
                //No data
                if (orderRecord == null)
                {
                    ret.ReturnCode = ReturnCode.Error_202;
                    return ret;
                }
                ret.Record = orderRecord;

                //Update requested status
                int timeOutIn = await SettingMaster.GetInt1("017");
                if (timeOutIn == 0) timeOutIn = 30;
                //
                orderRecord.IsPayRequest = true;
                orderRecord.RequestTime = DateTime.UtcNow;
                orderRecord.ExpiredTime = DateTime.UtcNow.AddMinutes(timeOutIn);
                //
                await orderRecord.SaveAsync();
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "VnPayService", "InitPayment", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            //
            return ret;
        }
        /// <summary>
        /// Get redirect payment link
        /// </summary>
        /// <param name="paymentChannel"></param>
        /// <param name="ipAddress"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<string> Gen_PaymentRedirectLink(string ipAddress, mdSaleOrder record)
        {
            string paymentUrl = "";
            //
            try
            {
                //Build URL for VNPAY
                VnPayLibrary vnpay = new VnPayLibrary();

                //Config
                string vnp_Url = await SettingMaster.GetString1("002"); // Payment url
                string vnp_Returnurl = await SettingMaster.GetString1("003");
                string vnp_TmnCode = await SettingMaster.GetString1("004"); //Ma website
                string vnp_HashSecret = await SettingMaster.GetString1("005"); //Chuoi bi mat
                                                                               //
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", (record.PaymentAmount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", ipAddress);
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan mua bao hiem {record.ProductName.RemoveVietnameseSign()}");
                vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", record.TransactionID); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                //Add Params of 2.1.0 Version
                //vnpay.AddRequestData("vnp_ExpireDate", txtExpire.Text);
                //Billing
                vnpay.AddRequestData("vnp_Bill_Mobile", record.CusPhone);
                vnpay.AddRequestData("vnp_Bill_Email", record.CusEmail);
                if (!String.IsNullOrEmpty(record.CusFullname))
                {
                    var indexof = record.CusFullname.IndexOf(' ');
                    if (indexof > 0)
                    {
                        vnpay.AddRequestData("vnp_Bill_FirstName", record.CusFullname.Substring(0, indexof));
                        vnpay.AddRequestData("vnp_Bill_LastName", record.CusFullname.Substring(indexof + 1, record.CusFullname.Length - indexof - 1));
                    }
                    else
                    {
                        vnpay.AddRequestData("vnp_Bill_FirstName", record.CusFullname);
                        vnpay.AddRequestData("vnp_Bill_LastName", record.CusFullname);
                    }
                }
                vnpay.AddRequestData("vnp_Bill_Address", record.Address);
                vnpay.AddRequestData("vnp_Bill_City", record.CityName);
                vnpay.AddRequestData("vnp_Bill_Country", "Vietnam");
                vnpay.AddRequestData("vnp_Bill_State", "");
                //
                paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "VnPayService", "Gen_PaymentRedirectLink", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return paymentUrl;
        }
        /// <summary>
        /// Check signature
        /// </summary>
        /// <param name="responseData"></param>
        /// <returns></returns>
        public async Task<bool> Check_Signature(Dictionary<string, string> responseData)
        {
            try
            {
                var vnp_SecureHash = "";
                VnPayLibrary vnpay = new VnPayLibrary();

                //get all querystring data
                foreach (var param in responseData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(param.Key, param.Value);
                        //vnp_SecureHash
                        if (param.Key == "vnp_SecureHash")
                        {
                            vnp_SecureHash = param.Value;
                        }
                    }
                }

                string vnp_HashSecret = await SettingMaster.GetString1("005"); //Chuoi bi mat

                //vnp_SecureHash
                return vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finish payment
        /// </summary>
        /// <param name="result"></param>
        /// <param name="queryData"></param>
        /// <returns></returns>
        public async Task<FinishPaymentResult> FinishPayment(VnPayResult result, Dictionary<string, string> queryData)
        {
            var ret = new FinishPaymentResult();
            ret.ReturnCode = ReturnCode.OK;
            //
            try
            {
                //Check signature
                var checkResult = await Check_Signature(queryData);
                if (!checkResult)
                {
                    ret.ReturnCode = ReturnCode.Error_201; //Invalid response
                    ret.ErrorMessage = "Lỗi bảo mật";
                    return ret;
                }

                //Security check
                string vnp_TmnCode = await SettingMaster.GetString1("004"); //Ma website

                //vnp_TmnCode
                if (vnp_TmnCode != result.vnp_TmnCode)
                {
                    ret.ReturnCode = ReturnCode.Error_201; //Invalid response
                    ret.ErrorMessage = "Lỗi bảo mật";
                    return ret;
                }

                //Get SaleOrder
                var record = await DB.Find<mdSaleOrder>()
                                     .Match(x => x.TransactionID == result.vnp_TxnRef)
                                     .Match(x => x.IsPayRequest == true)
                                     .Match(x => x.IsPayDone == false)
                                     .Match(x => x.IsPayError == false)
                                     .Match(x => x.ExpiredTime >= DateTime.UtcNow)
                                     .ExecuteFirstAsync();
                if (record == null)
                {
                    ret.ReturnCode = ReturnCode.Error_202; //Order not found
                    ret.ErrorMessage = "Đơn hàng đã xử lý xong trước đó hoặc đã quá thời gian thanh toán cho phép";
                    return ret;
                }
                ret.Record = record;

                //vnp_Amount
                if ((record.PaymentAmount * 100).ToString() != result.vnp_Amount.ToString())
                {
                    ret.ReturnCode = ReturnCode.Error_203; //Diff amount
                    ret.ErrorMessage = "Số tiền đơn hàng không khớp";
                    return ret;
                }

                //Update Payment Done
                record.PaymentTime = DateTime.UtcNow;
                if (result.vnp_ResponseCode == "00")
                {
                    //Success
                    record.IsPayDone = true;
                    record.IsPayError = false;
                }
                else
                {
                    //Failed
                    record.IsPayDone = false;
                    record.IsPayError = true;
                    //
                    ret.ReturnCode = ReturnCode.Error_204; //Error from VnPay
                    ret.ErrorMessage = VnPay_ErrorDic[result.vnp_ResponseCode];
                }
                record.PaymentRefID = result.vnp_TransactionNo;
                record.PaymentResponseCode = result.vnp_ResponseCode;
                //PaymentResponseData
                var resData = new Dictionary<string, string>();
                resData.Add("vnp_BankCode", result.vnp_BankCode);
                resData.Add("vnp_BankTranNo", result.vnp_BankTranNo);
                resData.Add("vnp_CardType", result.vnp_CardType);
                resData.Add("vnp_PayDate", result.vnp_PayDate);
                resData.Add("vnp_TransactionNo", result.vnp_TransactionNo);
                resData.Add("vnp_ResponseCode", result.vnp_ResponseCode);
                resData.Add("vnp_TxnRef", result.vnp_TxnRef);
                record.PaymentResponseData = MyJson.ToKeyPairJsonString(resData);
                //
                await record.SaveAsync();

                //Payment failed
                if (record.IsPayError) return ret;

                //
                //Call insurance provider to issuer Certificate
                //
                var issueRes = await IssueCertificate(record);
                ret.ReturnCode = issueRes.ReturnCode;
                ret.ErrorMessage = issueRes.ErrorMessage;

                //Return record
                ret.Record = record;

                //Update Discount Code
                if (ret.ReturnCode == ReturnCode.OK)
                {
                    SaleOrderService.Update_DiscountCode(record);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "FinishVnPay", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            //
            return ret;
        }

        public async Task<CallApiReturn> IssueCertificate(mdSaleOrder order)
        {
            var ret = new CallApiReturn();
            ret.ReturnCode = ReturnCode.OK;
            try
            {
                //BMI
                if (order.VendorID == MyConstant.Vendor_BMI)
                {
                    ret = await _ebaoService.eBao_CreateToIssue_TNDS(order);
                }

                //BHV
                if (order.VendorID == MyConstant.Vendor_BHV)
                {
                    ret = await _bhvService.Create_MotorTNDS(order);
                }


            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "IssueCertificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            return ret;
        }



    }
}
