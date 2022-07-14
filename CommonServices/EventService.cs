using Cores.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using MongoDB.Entities;
using Event.Services;
using Cores.Grpc.Client;
using Server.Common;

namespace Common.Services
{
    public static class EventService
    {
        //ClientApp
        public static void Public(string Publicer,
                                  string Subcriber,
                                  string EventName,
                                  string JsonStringData,
                                  int CallType,
                                  bool NeedAlarm = true,
                                  int MaxRetryCount = 0)
        {
            try
            {
                //Request
                var requestPub = new Event.Services.PublicEvents_Request();
                requestPub.Credential = new Event.Services.UserCredential() { ApiKey = GrpcCredential.ApiKey };
                //
                requestPub.Publicer = Publicer;
                requestPub.Subcriber = Subcriber;
                requestPub.EventName = EventName;
                requestPub.JsonStringData = JsonStringData;
                requestPub.CallType = CallType;
                requestPub.MaxRetryCount = MaxRetryCount;
                requestPub.NeedAlarm = NeedAlarm;
                //Reponse
                GrpcClientFactory.CallServiceFireForget(ServiceList.Event, async channel =>
                {
                    try
                    {
                        var client = new grpcEventService.grpcEventServiceClient(channel);
                        await client.PublicEventsAsync(requestPub);
                    }
                    catch { }
                });
                //
                //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}: Public {EventName}");
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "EventService", "Public", "Exception", 500, ex.Message);
            }
        }


        //
    }
}
