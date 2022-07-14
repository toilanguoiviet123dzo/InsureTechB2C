using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Database.Models;
using Insure.Services;
using MongoDB.Entities;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Text;
using CoreService;
using Cores.Utilities;
using System.Data;
using Cores.Grpc.Client;
using Server.Common;
using Common.Services;

namespace CoreService
{
    public class InsureService : grpcInsureService.grpcInsureServiceBase
    {
        private readonly ILogger<InsureService> _logger;

        public InsureService(ILogger<InsureService> logger)
        {
            _logger = logger;
        }
        //-------------------------------------------------------------------------------------------------------/
        // HealthCheck
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.String_Response> HealthCheck(Insure.Services.Empty_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            //
            try
            {
                //DB check
                var record = await DB.Find<mdSettingMaster>()
                                     .ExecuteFirstAsync();
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // SubcribeEvents
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.Empty_Response> SubcribeEvents(Insure.Services.SubcribeEvents_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.Empty_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            //
            try
            {
                //ServiceList
                if (request.EventName == MyConstant.EventName_ServiceList)
                {
                    await GrpcClientFactory.ReloadServiceListConfig();
                }

                //SettingMaster
                if (request.EventName == MyConstant.EventName_SettingMaster)
                {
                    await SettingMaster.Load_Setting();
                }

            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "GameService", "SubcribeEvents", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }


    }//end class
}//end name space
