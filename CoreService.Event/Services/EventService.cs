using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Database.Models;
using MongoDB.Entities;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Cores.Utilities;
using System.Data;
using Cores.Grpc.Client;
using Server.Common;
using Common.Services;
using Insure.Services;
using Event.Services;
using Cores.Helpers;

namespace CoreService
{
    public class EventService : grpcEventService.grpcEventServiceBase
    {
        private readonly ILogger<EventService> _logger;

        public EventService(ILogger<EventService> logger)
        {
            _logger = logger;
        }

        //-------------------------------------------------------------------------------------------------------/
        // PublicEvents		
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Event.Services.Empty_Response> PublicEvents(PublicEvents_Request request, ServerCallContext context)
        {
            var response = new Event.Services.Empty_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                var newRecord = new AppEventModel();
                //
                //Default retry count
                if (request.MaxRetryCount == 0)
                {
                    request.MaxRetryCount = 1000;
                }
                //
                ClassHelper.CopyPropertiesData(request, newRecord);
                //
                newRecord.ID = MyCodeGenerator.GenTransactionID();
                newRecord.CallType = MyConstant.EventCallType_2202;
                newRecord.ErrorFlag = false;
                newRecord.ErrorMessage = "";
                newRecord.StopAlarm = false;
                //
                EventCached.AddOrUpdate(newRecord);
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "EventService", "PublicEvents", "1", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

    }//end class
}//end name space
