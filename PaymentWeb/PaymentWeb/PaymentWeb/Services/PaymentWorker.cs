using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Entities;
using Server.Common;
using Cores.Helpers;
using Server.Common;
using Common.Services;
using Database.Models;

namespace PaymentWeb.Services
{
    public class PaymentWorker : BackgroundService
    {
        private readonly ILogger<PaymentWorker> _logger;
        private IConfiguration _configuration;
        private int DelayTime = 300; //second
        //
        public PaymentWorker(ILogger<PaymentWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private async Task Init()
        {
            try
            {
                //Get config from DB
                var setting = await SettingMaster.GetSetting("007");
                if (setting != null && setting.IntValue1 != 0)
                {
                    if (setting.IntValue1 != 0) DelayTime = setting.IntValue1;
                }
                else
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "SettingMaster", "007", "SettingMaster", ReturnCode.Error_ByServer, "SettingMaster.GetSetting_007: Not found");
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentWorker", "Init", "1", ReturnCode.Error_ByServer, ex.Message);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        //Init
                        await Init();
                        //
                        await Remove_TimemoutRequest();
                        await Remove_ErrorPay();
                        await Backup_Log();
                    }
                    catch (Exception ex)
                    {
                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentWorker", "ExecuteAsync", "1", ReturnCode.Error_ByServer, ex.Message);
                    }
                    //Delay time
                    await Task.Delay(1000 * DelayTime, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentWorker", "ExecuteAsync", "1", ReturnCode.Error_ByServer, ex.Message);
            }
        }
        //
        private async Task Remove_TimemoutRequest()
        {
            try
            {
                //Record chua duoc xu ly + ExpiredTime > DateTime.UtcNow
                var queueRecords = await DB.Find<mdSaleOrder>()
                                         .Match(a => a.IsPayDone == false)
                                         .Match(a => a.IsPayError == false)
                                         .Match(a => a.ExpiredTime < DateTime.UtcNow)
                                         .ExecuteAsync();
                //
                if (queueRecords != null && queueRecords.Count > 0)
                {
                    var blockQueueRecordIDs = new List<string>();
                    //
                    foreach (var record in queueRecords)
                    {
                        blockQueueRecordIDs.Add(record.ID);
                    }
                    //Batch delete
                    await DB.DeleteAsync<mdSaleOrder>(blockQueueRecordIDs);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentWorker", "Remove_TimemoutRequest", "1", ReturnCode.Error_ByServer, ex.Message);
            }
        }

        private async Task Remove_ErrorPay()
        {
            try
            {
                //Canceled
                var queueRecords = await DB.Find<mdSaleOrder>()
                                         .Match(a => a.IsPayError)
                                         .ExecuteAsync();
                //
                if (queueRecords != null && queueRecords.Count > 0)
                {
                    var blockQueueRecordIDs = new List<string>();
                    //
                    foreach (var record in queueRecords)
                    {
                        blockQueueRecordIDs.Add(record.ID);
                    }
                    //Batch delete
                    await DB.DeleteAsync<mdSaleOrder>(blockQueueRecordIDs);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentWorker", "Remove_Cancel", "1", ReturnCode.Error_ByServer, ex.Message);
            }
        }
        //
        private async Task Backup_Log()
        {
            try
            {
                //Record da xu ly: thanh cong va da qua thoi gian timeout
                var queueRecords = await DB.Find<mdSaleOrder>()
                                         .Match(a => a.IsPayDone)
                                         .Match(a => a.IsProcessDone)
                                         .ExecuteAsync();
                //
                if (queueRecords != null && queueRecords.Count > 0)
                {
                    foreach (var queueRecord in queueRecords)
                    {
                        //Insert to log
                        var logRecord = new mdSaleOrderLog();
                        ClassHelper.CopyPropertiesData(queueRecord, logRecord);
                        //
                        await logRecord.SaveAsync();

                        //Delete queue
                        await DB.DeleteAsync<mdSaleOrder>(queueRecord.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "PaymentWorker", "Backup_Log", "1", ReturnCode.Error_ByServer, ex.Message);
            }
        }
        //
    }
}
