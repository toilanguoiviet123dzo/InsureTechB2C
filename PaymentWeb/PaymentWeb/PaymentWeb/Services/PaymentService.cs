﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using Cores.Utilities;
using Cores.Helpers;
using Server.Common;
using Database.Models;
using Common.Services;

namespace PaymentWeb.Services
{
    public class PaymentService
    {
        private VnPayService _vnPay;
        private eBaoService _ebaoService;
        private BHVService _bhvService;
        public PaymentService(VnPayService vnPay,
                              eBaoService ebaoService,
                              BHVService bhvService)
        {
            _vnPay = vnPay;
            _ebaoService = ebaoService;
            _bhvService = bhvService;
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
                if (paymentChannelID == MyConstant.PaymentChannel_VnPay)
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "DonateOrder", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
            //
            return ret;
        }

        /// <summary>
        /// Cash payment
        /// </summary>
        /// <param name="initOrderToken"></param>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public async Task<FinishPaymentResult> CashPayment(string initOrderToken, string transactionID)
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
                if (PaymentChannelID == MyConstant.PaymentChannel_VnPay)
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
                if (PaymentChannelID == MyConstant.PaymentChannel_VnPay)
                {
                    ret = await _vnPay.FinishPayment(result, queryData);

                    //Issue certificate
                    if (ret.ReturnCode == ReturnCode.OK)
                    {
                        var issueReturn = await IssueCertificate(ret.Record);
                        ret.ReturnCode = issueReturn.ReturnCode;
                        ret.ErrorMessage = issueReturn.ErrorMessage;
                    }
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
        public async Task<CallApiReturn> IssueCertificate(mdSaleOrder order)
        {
            var ret = new CallApiReturn();
            ret.ReturnCode = ReturnCode.OK;
            try
            {
                //DateTime
                order.OrderDate = DateTime.UtcNow;
                order.PolicyDate = DateTime.UtcNow;

                //BMI
                if (order.VendorID == MyConstant.Vendor_BMI)
                {
                    //Motor & Autor
                    ret = await _ebaoService.eBao_CreateToIssue_TNDS(order);
                }

                //BHV
                if (order.VendorID == MyConstant.Vendor_BHV)
                {
                    //TNDS motor
                    if (order.ProductID == MyConstant.Product_MotorBHV)
                    {
                        ret = await _bhvService.Create_MotorTNDS(order);
                    }

                    //TNDS auto
                    if (order.ProductID == MyConstant.Product_AutoBHV)
                    {
                        ret = await _bhvService.Create_AutoTNDS(order);
                    }

                    //TNDS auto
                    if (order.ProductID == MyConstant.Product_FlashCare)
                    {
                        ret = await _bhvService.Create_FlashCare(order);
                    }
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "IssueCertificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
                ret.ReturnCode = ReturnCode.Error_ByServer;
            }
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