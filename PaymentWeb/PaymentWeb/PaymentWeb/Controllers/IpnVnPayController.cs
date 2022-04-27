using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using BlazorApp.Server.Models;
using BlazorApp.Server.Common;
using PaymentWeb.Services;

namespace PaymentWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpnVnPayController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        public IpnVnPayController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<VnPayIpnResponseModel> ReceiveResult(double vnp_Amount,
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "ReceiveResult", vnp_TxnRef, ReturnCode.Error_ByServer, "VpPay reach");

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
                    return response;
                }

                //Check signature
                var checkResult = await VnPayService.Check_Signature(querryData);
                if (!checkResult)
                {
                    response.RspCode = "97";
                    response.Message = "Invalid Checksum";
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
                    return response;
                }
                else
                {

                    //already confirmed
                    if (record.IsPayDone)
                    {
                        response.RspCode = "02";
                        response.Message = "Order already confirmed";
                        return response;
                    }

                    //Amount not match
                    if (record.PaymentAmount * 100 != vnp_Amount)
                    {
                        response.RspCode = "04";
                        response.Message = "Invalid amount";
                        return response;
                    }
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
                var ret = await _paymentService.FinishPayment(MyConstant.PyamentChannel_VnPay, request, querryData);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "IpnVnPay", "Update_Finish", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
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
