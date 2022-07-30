using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using Database.Models;
using Server.Common;
using PaymentWeb.Services;
using Cores.Utilities;

namespace PaymentWeb.Controllers
{

    [ApiController]
    public class IpnVnPayController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly VnPayService _vnPayService;
        public IpnVnPayController(PaymentService paymentService,
                                  VnPayService vnPayService)
        {
            _paymentService = paymentService;
            _vnPayService = vnPayService;
        }

        [Route("api/IpnVnPay")]
        [HttpGet]
        public async Task<VnPayIpnResponseModel> IpnVnPay(double vnp_Amount,
                                                          string vnp_BankCode,
                                                          string vnp_OrderInfo,
                                                          string vnp_PayDate,
                                                          string vnp_ResponseCode,
                                                          string vnp_SecureHash,
                                                          string vnp_TmnCode,
                                                          string vnp_TransactionNo,
                                                          string vnp_TransactionStatus,
                                                          string vnp_TxnRef)
        {
            var response = new VnPayIpnResponseModel();
            try
            {
                //MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", vnp_TxnRef, ReturnCode.Error_ByServer, "VpPay reach");

                //get all querystring data
                var querryData = new Dictionary<string, string>();
                querryData.Add("vnp_TmnCode", vnp_TmnCode);
                querryData.Add("vnp_TxnRef", vnp_TxnRef);
                querryData.Add("vnp_Amount", vnp_Amount.ToString());
                querryData.Add("vnp_OrderInfo", vnp_OrderInfo);
                querryData.Add("vnp_ResponseCode", vnp_ResponseCode);
                querryData.Add("vnp_BankCode", vnp_BankCode);
                querryData.Add("vnp_PayDate", vnp_PayDate);
                querryData.Add("vnp_TransactionNo", vnp_TransactionNo);
                querryData.Add("vnp_TransactionStatus", vnp_TransactionStatus);
                querryData.Add("vnp_SecureHash", vnp_SecureHash);

                //Failed from vnPay
                if (vnp_ResponseCode != "00")
                {
                    response.RspCode = "00";
                    response.Message = "Confirm Success";
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", "response", ReturnCode.Error_ByServer, MyJson.Serialize(response));
                    return response;
                }

                //Check signature
                var checkResult = await _vnPayService.Check_Signature(querryData);
                if (!checkResult)
                {
                    response.RspCode = "97";
                    response.Message = "Invalid Checksum";
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", "response", ReturnCode.Error_ByServer, MyJson.Serialize(response));
                    return response;
                }

                //Transaction not found
                var record = await DB.Find<mdSaleOrder>()
                                    .Match(x => x.TransactionID == vnp_TxnRef)
                                    .ExecuteFirstAsync();
                if (record == null)
                {
                    record = await DB.Find<mdSaleOrderLog>()
                                    .Match(x => x.TransactionID == vnp_TxnRef)
                                    .ExecuteFirstAsync();
                }
                if (record == null)
                {
                    response.RspCode = "01";
                    response.Message = "Order Not Found";
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", "response", ReturnCode.Error_ByServer, MyJson.Serialize(response));
                    return response;
                }
                else
                {
                    //Amount not match
                    if (record.PaymentAmount * 100 != vnp_Amount)
                    {
                        response.RspCode = "04";
                        response.Message = "Invalid amount";
                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", "response", ReturnCode.Error_ByServer, MyJson.Serialize(response));
                        return response;
                    }

                    //already confirmed > not check
                    //if (record.IsPayDone)
                    //{
                    //    response.RspCode = "02";
                    //    response.Message = "Order already confirmed";
                    //    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", "response", ReturnCode.Error_ByServer, MyJson.Serialize(response));
                    //    return response;
                    //}
                }

                //VnPayResult
                var request = new VnPayResult();
                request.vnp_TmnCode = vnp_TmnCode;
                request.vnp_TxnRef = vnp_TxnRef;
                request.vnp_Amount = vnp_Amount;
                request.vnp_OrderInfo = vnp_OrderInfo;
                request.vnp_ResponseCode = vnp_ResponseCode;
                request.vnp_BankCode = vnp_BankCode;
                request.vnp_PayDate = vnp_PayDate;
                request.vnp_TransactionNo = vnp_TransactionNo;
                request.vnp_TransactionStatus = vnp_TransactionStatus;
                request.vnp_SecureHash = vnp_SecureHash;

                //Finish payment
                Update_Finish(request, querryData);
            }
            catch (Exception ex)
            {
                response.RspCode = "99";
                response.Message = "Unknow error";
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return response;
        }
        /// <summary>
        /// Finish payment
        /// </summary>
        /// <param name="request"></param>
        /// <param name="querryData"></param>
        private async void Update_Finish(VnPayResult request, Dictionary<string, string> querryData)
        {
            try
            {
                //Sleep 20s: direct return is prioritized
                Thread.Sleep(20 * 1000);

                //Check is wherther done by direct return
                var record = await DB.Find<mdSaleOrder>()
                                     .Match(x => x.TransactionID == request.vnp_TxnRef)
                                     .Match(x => x.IsPayRequest == true)
                                     .Match(x => x.IsPayDone == false)
                                     .Match(x => x.IsPayError == false)
                                     .Match(x => x.ExpiredTime >= DateTime.UtcNow)
                                     .ExecuteFirstAsync();
                if (record == null) return;

                //Finish payment
                var ret = await _paymentService.FinishPayment(MyConstant.PaymentChannel_VnPay, request, querryData);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "Update_Finish", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
        }

        [Route("api/ReceiveZaloCode")]
        [HttpGet]
        public string ReceiveZaloCode(string code = "", string oa_id = "")
        {
            var response = "";
            try
            {
                response = $"Code:{code}; oa_id:{oa_id}";
                MyAppLog.WriteLog(MyConstant.LogLevel_Information, "Zalo", "ReceiveCode", code, ReturnCode.Error_ByServer, oa_id);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "Zalo", "ReceiveCode", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return response;
        }


        //
    }





    public class VnPayIpnResponseModel
    {
        public string RspCode { get; set; } = "00";
        public string Message { get; set; } = "Confirm Success";
    }

    //{
    //    "vnp_TmnCode": "vnp_TmnCode",
    //    "vnp_TxnRef": "vnp_TxnRef",
    //    "vnp_Amount": 100000,
    //    "vnp_OrderInfo": "vnp_OrderInfo",
    //    "vnp_ResponseCode": "vnp_ResponseCode",
    //    "vnp_BankCode": "vnp_BankCode",
    //    "vnp_BankTranNo": "vnp_BankTranNo",
    //    "vnp_CardType": "vnp_CardType",
    //    "vnp_PayDate": "vnp_PayDate",
    //    "vnp_TransactionNo": "vnp_TransactionNo",
    //    "vnp_TransactionStatus": "vnp_TransactionStatus",
    //    "vnp_SecureHash": "vnp_SecureHash"
    //}

}
