using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Entities;
using Insure.Services;
using Common.Services;
using Server.Common;
using Cores.Grpc.Client;

namespace CoreService
{
    public class EventWorker : BackgroundService
    {
        private readonly ILogger<EventWorker> _logger;
        private int DelayTime = 5; //second
        private bool IsReady = false;
        //
        public EventWorker(ILogger<EventWorker> logger)
        {
            _logger = logger;
        }

        private async Task Init()
        {
            try
            {
                //Get timer
                DelayTime = await SettingMaster.GetInt1("026");
                if (DelayTime == 0) DelayTime = 5; // 5 s
                IsReady = true;
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "EventWorker", "Init", "1", GrpcReturnCode.Error_ByServer, ex.Message);
                IsReady = false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //Init
                await Init();

                //Check ready
                if (!IsReady) return;
                //
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await Call_SubcribedService();
                    }
                    catch (Exception ex)
                    {
                        MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "EventWorker", "ExecuteAsync", "1", GrpcReturnCode.Error_ByServer, ex.Message);
                    }

                    //Delay time
                    await Task.Delay(1000 * DelayTime, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "EventWorker", "ExecuteAsync", "1", GrpcReturnCode.Error_ByServer, ex.Message);
            }
        }
        //
        private async Task Call_SubcribedService()
        {
            try
            {
                //Get events to call
                for (int i = EventCached.AppEvents.Count - 1; i >= 0; i--)
                {
                    var rec = EventCached.AppEvents[i];
                    //
                    //Check for call
                    if (!rec.CallDone && rec.RetryCount < rec.MaxRetryCount)
                    {
                        var result = await Call_SubcribedService(rec);
                        //
                        rec.ErrorFlag = result.ErrorFlag;
                        rec.ErrorMessage = result.ErrorMessage;
                        if (!rec.ErrorFlag)
                        {
                            rec.CallDone = true;
                        }
                        rec.RetryCount++;
                    }

                    //Check for remove
                    if (rec.CallDone || rec.RetryCount >= rec.MaxRetryCount)
                    {
                        EventCached.RemoveAt(i);
                    }
                    else
                    {
                        //Save
                        EventCached.AddOrUpdate(rec);
                    }
                }
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "EventWorker", "Call_SubcribedService", "1", GrpcReturnCode.Error_ByServer, ex.Message);
            }
            //
        }

        private async Task<SubcribeResult> Call_SubcribedService(AppEventModel record)
        {
            var result = new SubcribeResult();

            // Fire and wait to receive result
            if (record.CallType == MyConstant.EventCallType_3303)
            {
                //1. MobileGateway
                if (string.IsNullOrWhiteSpace(record.Subcriber) || record.Subcriber.Contains(ServiceList.Insure))
                {
                    //Request
                    var request = new Insure.Services.SubcribeEvents_Request();
                    request.Credential = new Insure.Services.UserCredential() { ApiKey = GrpcCredential.ApiKey };
                    request.EventName = record.EventName;
                    //Reponse
                    var response = await GrpcClientFactory.CallServiceAsync<Insure.Services.Empty_Response>(ServiceList.Insure, async channel =>
                    {
                        var client = new grpcInsureService.grpcInsureServiceClient(channel);
                        return await client.SubcribeEventsAsync(request);
                    });
                    //Error
                    if (response == null || response.ReturnCode != GrpcReturnCode.OK)
                    {
                        result.ErrorFlag = true;
                        result.ErrorMessage += $"Insure.{record.EventName}";
                    }
                }

                
            }
            //Fire and forget
            else
            {
                //1. MobileGateway
                if (string.IsNullOrWhiteSpace(record.Subcriber) || record.Subcriber.Contains(ServiceList.Insure))
                {
                    //Request
                    var request = new Insure.Services.SubcribeEvents_Request();
                    request.Credential = new Insure.Services.UserCredential() { ApiKey = GrpcCredential.ApiKey };
                    request.EventName = record.EventName;
                    //Reponse
                    GrpcClientFactory.CallServiceFireForget(ServiceList.Insure, async channel =>
                    {
                        try
                        {
                            var client = new grpcInsureService.grpcInsureServiceClient(channel);
                            await client.SubcribeEventsAsync(request);
                        }
                        catch { }
                    });
                }

                

            }
            //
            return result;
        }
        //
    }

    public class SubcribeResult
    {
        public bool ErrorFlag { get; set; } = false;
        public string ErrorMessage { get; set; } = "";
    }
}
