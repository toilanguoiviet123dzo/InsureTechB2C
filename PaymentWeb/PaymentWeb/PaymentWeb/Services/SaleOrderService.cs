using Server.Common;
using Database.Models;
using MongoDB.Entities;

namespace PaymentWeb.Services
{
    public class SaleOrderService
    {

        /// <summary>
        /// Update DiscountCode.TotalMaxQty
        /// </summary>
        /// <param name="order"></param>
        public static async void Update_DiscountCode(mdSaleOrder order)
        {
            try
            {
                //skip check
                if (string.IsNullOrWhiteSpace(order.DiscountCode)) return;

                var record = await DB.Find<mdDiscountCode>()
                                     .Match(x => x.DiscountCode == order.DiscountCode)
                                     .ExecuteFirstAsync();
                if (record != null)
                {
                    record.TotalMaxQty += 1;
                    //
                    await record.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentService", "Update_DiscountCode", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
        }
    }
}
