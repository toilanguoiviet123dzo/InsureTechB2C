using System;
using System.Threading.Tasks;
using BlazorApp.Server.Models;
using Cores.Utilities;
using MongoDB.Entities;

namespace BlazorApp.Server.Common
{
    public static class MyVoucher
    {
        public async static Task<string> GetVoucherNo(string voucherCode)
        {
            //Default Voucher No
            string currentYear = DateTime.Now.ToString("yy");
            string newVoucherNo = currentYear + "000001";
            //
            try
            {
                //Get from DB + 1
                var findRecords = await DB.Find<mdVoucherMaster>()
                                          .Match(a => a.VoucherCode == voucherCode)
                                          .ExecuteFirstAsync();
                //
                if (findRecords != null)
                {
                    //Check same year
                    if (!String.IsNullOrWhiteSpace(findRecords.CurrentVoucherNo) && currentYear == findRecords.CurrentVoucherNo.Left(2))
                    {
                        var nextSeq = findRecords.CurrentVoucherNo.Right(6).ToInt() + 1;
                        newVoucherNo = currentYear + nextSeq.ToString("000000");
                    }
                }
                else
                {
                    //Add new master record
                    var newRecord = new mdVoucherMaster();
                    newRecord.VoucherCode = voucherCode;
                    newRecord.CurrentVoucherNo = currentYear + "000000"; ;
                    newRecord.MinVoucherNo = "00000000";
                    newRecord.MaxVoucherNo = "99999999";
                    newRecord.ModifiedOn = DateTime.UtcNow;
                    newRecord.UpdMode = 1;
                    //
                    await newRecord.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                newVoucherNo = "";
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "Get_VoucherNo", "Exception", "Exception", 500, ex.Message);
            }
            //
            return newVoucherNo;
        }

        public async static Task<string> CommitVoucherNo(string voucherCode, string voucherNo)
        {
            string committedVoucherNo = voucherNo;
            //
            try
            {
                //Check for dublicated
                var findRecords = await DB.Find<mdVoucherMaster>()
                                          .Match(a => a.CurrentVoucherNo == voucherNo && a.VoucherCode == voucherCode)
                                          .ExecuteSingleAsync();
                //Gen new VoucherNo
                if (findRecords != null)
                {
                    var getNewVoucherRequest = new Admin.Services.String_Request();
                    getNewVoucherRequest.StringValue = voucherCode;
                    var newVoucherNo = await GetVoucherNo(voucherCode);
                    if (!string.IsNullOrWhiteSpace(newVoucherNo))
                    {
                        committedVoucherNo = newVoucherNo;
                    }
                }
                //Commit
                var updateRecord = await DB.Find<mdVoucherMaster>()
                                          .Match(a => a.VoucherCode == voucherCode)
                                          .ExecuteFirstAsync();
                if (updateRecord != null)
                {
                    updateRecord.CurrentVoucherNo = committedVoucherNo;
                    updateRecord.ModifiedOn = DateTime.UtcNow;
                    updateRecord.UpdMode = 2;
                    //
                    await updateRecord.SaveAsync();
                }
                else
                {
                    //Add new master record
                    var newRecord = new mdVoucherMaster();
                    newRecord.VoucherCode = voucherCode;
                    newRecord.CurrentVoucherNo = committedVoucherNo;
                    newRecord.MinVoucherNo = "00000000";
                    newRecord.MaxVoucherNo = "99999999";
                    newRecord.ModifiedOn = DateTime.UtcNow;
                    newRecord.UpdMode = 1;
                    //
                    await newRecord.SaveAsync();
                }
                //
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "CommitVoucherNo", "CommitVoucherNo", "Exception", 500, ex.Message);
                return "";
            }
            return committedVoucherNo;
        }

    }
}
