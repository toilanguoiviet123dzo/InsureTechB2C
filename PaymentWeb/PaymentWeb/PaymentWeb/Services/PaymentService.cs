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
    public static class PaymentService
    {
        //InitPayment
        public static async Task<int> InitPayment(string initOrderToken, string transactionID)
        {
            try
            {
                //201: UnAuthorized
                //202: No transaction found
                //Check security
                if (!MyTokenService.Check_InitOrderToken(initOrderToken, transactionID)) return ReturnCode.Error_201;

                //Check Transaction
                var orderRecord = await DB.Find<mdSaleOrder>()
                                          .Match(x => x.TransactionID == transactionID)
                                          .Match(x => x.IsPayRequest == false)
                                          .ExecuteSingleAsync();
                //No data
                if (orderRecord == null) return ReturnCode.Error_202;

                //Update requested status
                orderRecord.IsPayRequest = true;
                orderRecord.RequestTime = DateTime.UtcNow;
                //
                await orderRecord.SaveAsync();
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "InitPayment", "Exception", ReturnCode.Error_ByServer, ex.Message);
                return ReturnCode.Error_ByServer;
            }
            //
            return ReturnCode.OK;
        }

        //FinishPayment
        public static async Task<int> FinishPayment(string finishOrderToken, string transactionID)
        {
            try
            {
                //201: UnAuthorized
                //202: No transaction found
                //Check security
                if (!MyTokenService.Check_FinishOrderToken(finishOrderToken, transactionID)) return ReturnCode.Error_201;

                //Check Transaction
                var orderRecord = await DB.Find<mdSaleOrder>()
                                          .Match(x => x.TransactionID == transactionID)
                                          .Match(x => x.IsPayRequest == false)
                                          .ExecuteSingleAsync();
                //No data
                if (orderRecord == null) return ReturnCode.Error_202;

                //Update payDone status
                orderRecord.IsPayDone = true;
                orderRecord.PaymentTime = DateTime.UtcNow;
                //
                await orderRecord.SaveAsync();

                //Update successfull
                TaskHelper.RunBg( () =>
                {
                    try
                    {
                        //Issue Order to eBao




                        //Download certificate


                        
                    }
                    catch { }
                    //
                    return Task.FromResult(0);
                });
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "InitPayment", "Exception", ReturnCode.Error_ByServer, ex.Message);
                return ReturnCode.Error_ByServer;
            }
            //
            return ReturnCode.OK;
        }


        //
    }
}
