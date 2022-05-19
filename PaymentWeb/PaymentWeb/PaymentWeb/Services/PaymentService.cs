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
        private VnPayService _vnPay;
        private eBaoService _ebaoService;
        public PaymentService(VnPayService vnPay,
                              eBaoService ebaoService)
        {
            _vnPay = vnPay;
            _ebaoService = ebaoService;
        }
        /// <summary>
        /// Init payment
        /// </summary>
        /// <param name="initOrderToken"></param>
        /// <param name="transactionID"></param>
        /// <param name="paymentChannelID"></param>
        /// <returns></returns>
        public async Task<InitPaymentResult> InitPayment(string paymentChannelID, string initOrderToken, string transactionID)
        {
            var ret = new InitPaymentResult();
            ret.ReturnCode = ReturnCode.OK;
            //
            try
            {
                //VnPay
                if (paymentChannelID == MyConstant.PyamentChannel_VnPay)
                {
                    return await _vnPay.InitPayment(initOrderToken, transactionID);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "InitPayment", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            //
            return ret;
        }

        /// <summary>
        /// Donate order
        /// </summary>
        /// <param name="initOrderToken"></param>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public async Task<FinishPaymentResult> DonateOrder(string initOrderToken, string transactionID)
        {
            var ret = new FinishPaymentResult();
            ret.ReturnCode = ReturnCode.OK;
            //
            try
            {
                //201: UnAuthorized
                //202: No transaction found
                //Check security
                if (!MyTokenService.Check_InitOrderToken(initOrderToken, transactionID))
                {
                    Console.WriteLine(transactionID);

                    ret.ReturnCode = ReturnCode.Error_201;
                    ret.ErrorMessage = "Lỗi bảo mật";
                    return ret;
                }

                //Check Transaction
                var record = await DB.Find<mdSaleOrder>()
                                          .Match(x => x.TransactionID == transactionID)
                                          .Match(x => x.IsPayRequest == false)
                                          .ExecuteFirstAsync();
                //No data
                if (record == null)
                {
                    ret.ReturnCode = ReturnCode.Error_202;
                    ret.ErrorMessage = "Không tồn tại giao dịch tương ứng";
                    return ret;
                }
                ret.Record = record;

                //Update donate
                int timeOutIn = await SettingMaster.GetInt1("017");
                if (timeOutIn == 0) timeOutIn = 30;
                //
                record.IsPayRequest = true;
                record.RequestTime = DateTime.UtcNow;
                record.ExpiredTime = DateTime.UtcNow.AddMinutes(timeOutIn);
                //Payment done
                record.IsPayDone = true;
                record.IsPayError = false;
                //Save
                await record.SaveAsync();

                //Issue Order to eBao
                var issueRes = await _ebaoService.eBao_CreateToIssue_TNDS(record);
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "DonateOrder", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            //
            return ret;
        }

        /// <summary>
        /// Get payment redirect link
        /// </summary>
        /// <param name="PaymentChannelID"></param>
        /// <param name="ipAddress"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<string> Gen_PaymentRedirectLink(string PaymentChannelID, string ipAddress, mdSaleOrder record)
        {
            string paymentUrl = "";
            //
            try
            {
                //VnPay
                if (PaymentChannelID == MyConstant.PyamentChannel_VnPay)
                {
                    return await _vnPay.Gen_PaymentRedirectLink(ipAddress, record);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "VnPayService", "Gen_PaymentRedirectLink", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            //
            return paymentUrl;
        }


        
        /// <summary>
        /// Finish payment
        /// </summary>
        /// <param name="PaymentChannelID"></param>
        /// <param name="result"></param>
        /// <param name="queryData"></param>
        /// <returns></returns>
        public async Task<FinishPaymentResult> FinishPayment(string PaymentChannelID, VnPayResult result, Dictionary<string, string> queryData)
        {
            var ret = new FinishPaymentResult();
            ret.ReturnCode = ReturnCode.OK;
            //
            try
            {
                //VnPay
                if (PaymentChannelID == MyConstant.PyamentChannel_VnPay)
                {
                    return await _vnPay.FinishPayment(result, queryData);
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
        


        //
    }
}

public class InitPaymentResult
{
    public int ReturnCode { get; set; }
    public mdSaleOrder Record { get; set; } = new mdSaleOrder();
}

public class TaskResult
{
    public int ReturnCode { get; set; }
    public string ErrorMessage { get; set; } = "";
    public dynamic? Data { get; set; }
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
    public string vnp_TxnRef { get; set; } = ""; //TransactionID from merchant
    public double vnp_Amount { get; set; }
    public string vnp_OrderInfo { get; set; } = "";
    public string vnp_ResponseCode { get; set; } = ""; //00: Success
    public string vnp_BankCode { get; set; } = "";
    public string vnp_BankTranNo { get; set; } = "";
    public string vnp_CardType { get; set; } = "";
    public string vnp_PayDate { get; set; } = "";   //yyyyMMddHHmmss
    public string vnp_TransactionNo { get; set; } = ""; // VnPay transactionNo
    public string vnp_TransactionStatus { get; set; } = ""; //00: Success
    public string vnp_SecureHash { get; set; } = "";
}