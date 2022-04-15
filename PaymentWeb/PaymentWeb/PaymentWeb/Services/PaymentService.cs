using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using Cores.Utilities;
using Cores.Helpers;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;

namespace PaymentWeb.Services
{
    public class PaymentService
    {
        private eBaoService _ebaoService;
        public PaymentService(eBaoService ebaoService)
        {
            _ebaoService = ebaoService;
        }
        //
        public Dictionary<string, string> VnPay_ErrorDic { get; set; } = new Dictionary<string, string>()
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

        //InitPayment
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "InitPayment", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            //
            return ret;
        }

        public async Task<string> Gen_PaymentRedirectLink(string paymentChannel, string ipAddress, mdSaleOrder record)
        {
            string paymentUrl = "";
            //VnPay
            if (paymentChannel == "1")
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
                vnpay.AddRequestData("vnp_Amount", (record.UnitPrice * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
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

                // Invoice
                //vnpay.AddRequestData("vnp_Inv_Phone", txt_inv_mobile.Text.Trim());
                //vnpay.AddRequestData("vnp_Inv_Email", txt_inv_email.Text.Trim());
                //vnpay.AddRequestData("vnp_Inv_Customer", txt_inv_customer.Text.Trim());
                //vnpay.AddRequestData("vnp_Inv_Address", txt_inv_addr1.Text.Trim());
                //vnpay.AddRequestData("vnp_Inv_Company", txt_inv_company.Text);
                //vnpay.AddRequestData("vnp_Inv_Taxcode", txt_inv_taxcode.Text);
                //vnpay.AddRequestData("vnp_Inv_Type", cbo_inv_type.SelectedItem.Value);
                //
                paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            }
            //
            return paymentUrl;
        }

        public async Task<FinishPaymentResult> FinishVnPay(VnPayResult result, IQueryCollection query)
        {
            var ret = new FinishPaymentResult();
            ret.ReturnCode = ReturnCode.OK;
            //
            try
            {
                VnPayLibrary vnpay = new VnPayLibrary();

                //get all querystring data
                foreach (var param in query)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(param.Key, param.Value);
                    }
                }

                //Security check
                string vnp_TmnCode = await SettingMaster.GetString1("004"); //Ma website
                string vnp_HashSecret = await SettingMaster.GetString1("005"); //Chuoi bi mat

                //vnp_TmnCode
                if (vnp_TmnCode != result.vnp_TmnCode)
                {
                    ret.ReturnCode = ReturnCode.Error_201; //Invalid response
                    ret.ErrorMessage = "Lỗi bảo mật";
                    return ret;
                }
                //vnp_SecureHash
                bool checkSignature = vnpay.ValidateSignature(result.vnp_SecureHash, vnp_HashSecret);
                if (!checkSignature)
                {
                    ret.ReturnCode = ReturnCode.Error_201; //Invalid response
                    ret.ErrorMessage = "Lỗi bảo mật";
                    return ret;
                }


                //Get SaleOrder
                var record = await DB.Find<mdSaleOrder>()
                                     .Match(x => x.TransactionID == result.vnp_TxnRef)
                                     .ExecuteFirstAsync();
                if (record == null)
                {
                    ret.ReturnCode = ReturnCode.Error_202; //Order not found
                    ret.ErrorMessage = "Không tồn tại đơn hàng tương ứng";
                    return ret;
                }
                ret.Record = record;

                //vnp_Amount
                if ((record.UnitPrice * 100).ToString() != result.vnp_Amount)
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


                //Update successfull
                //Issue Order to eBao
                try
                {
                    var res = await _ebaoService.CreateToIssue(record);
                    if (res != null && res.data != null && res.data.processStatus == "PASS")
                    {
                        //Success
                        record.IsProcessDone = true;
                        record.IsProcessError = false;
                        record.PolicyID = res.data.policyId;
                        record.PolicyNo = res.data.policyNo;
                        record.QuoteNo = res.data.quoteNo;

                        //Download certificate
                        record.CertificateLink = await _ebaoService.GetCertificateLink(record.PolicyID);
                    }
                    else
                    {
                        //Failed
                        record.IsProcessDone = false;
                        record.IsProcessError = true;
                        if (res != null && res.data != null) record.ProcessErrorMessage = res.data.processMsg;
                        //Log
                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "CreateToIssue", "Failed", ReturnCode.Error_ByServer, $"Call eBao api failed for {record.TransactionID}_{record.LicensePlate}_{record.CusPhone}_{record.CusFullname}");
                    }
                    await record.SaveAsync();
                }
                catch (Exception ex)
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "CreateToIssue", "Exception", ReturnCode.Error_ByServer, ex.Message);
                }

                //Return record
                ret.Record = record;
                //TaskHelper.RunBg(async () =>
                //{
                //    try
                //    {

                //    }
                //    catch { }
                //    //
                //});
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "FinishVnPay", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            //
            return ret;
        }



        //
    }
}

public class InitPaymentResult
{
    public int ReturnCode { get; set; }
    public mdSaleOrder Record { get; set; } = new mdSaleOrder();
}

public class FinishPaymentResult
{
    public int ReturnCode { get; set; } = 200;
    public string ErrorMessage { get; set; } = "";
    public mdSaleOrder Record { get; set; } = new mdSaleOrder();
}

public class VnPayResult
{
    public string vnp_TmnCode { get; set; } = "";
    public string vnp_Amount { get; set; } = "";
    public string vnp_BankCode { get; set; } = "";
    public string vnp_BankTranNo { get; set; } = "";
    public string vnp_CardType { get; set; } = "";
    public string vnp_PayDate { get; set; } = "";
    public string vnp_OrderInfo { get; set; } = "";
    public string vnp_TransactionNo { get; set; } = "";
    public string vnp_ResponseCode { get; set; } = "";
    public string vnp_TransactionStatus { get; set; } = "";
    public string vnp_TxnRef { get; set; } = "";
    public string vnp_SecureHashType { get; set; } = "";
    public string vnp_SecureHash { get; set; } = "";
}