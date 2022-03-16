using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Entities;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using Google.Protobuf;
using System.Text;
using Grpc.Net.Client;
using Cores.Grpc.Client;
using Cores.Helpers;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;
using Cores.Utilities;
using Insure.Services;

namespace BlazorApp.Server.Services
{
    public class InsureService : grpcInsureService.grpcInsureServiceBase
    {
        private readonly ILogger<InsureService> _logger;

        public InsureService(ILogger<InsureService> logger)
        {
            _logger = logger;
        }
        //-------------------------------------------------------------------------------------------------------/
        // SaveProduct
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.String_Response> SaveProduct(SaveProduct_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                response.StringValue = request.Record.ID;
                //Addnew or update
                if (request.Record.UpdMode == 1 || request.Record.UpdMode == 2)
                {
                    mdProduct saveRecord = new mdProduct();
                    //Update
                    if (request.Record.UpdMode == 2)
                    {
                        saveRecord = await DB.Find<mdProduct>()
                                             .MatchID(request.Record.ID)
                                             .ExecuteSingleAsync();
                    }
                    if (saveRecord != null)
                    {
                        ClassHelper.CopyPropertiesData(request.Record, saveRecord);
                        //GenerateNewID
                        if (request.Record.UpdMode == 1) saveRecord.ID = saveRecord.GenerateNewID();
                        saveRecord.ModifiedOn = DateTime.UtcNow;
                        //
                        await saveRecord.SaveAsync();
                        //
                        response.StringValue = saveRecord.ID;
                    }
                }
                //Delete
                if (request.Record.UpdMode == 3)
                {
                    await DB.DeleteAsync<mdProduct>(request.Record.ID);
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "SaveProduct", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetProduct
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.GetProduct_Response> GetProduct(String_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.GetProduct_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                var gotRecord = await DB.Find<mdProduct>()
                                    .Match(x => x.ProductID == request.StringValue)
                                    .ExecuteSingleAsync();

                if (gotRecord != null)
                {
                    response.Record = new grpcProductModel();
                    ClassHelper.CopyPropertiesData(gotRecord, response.Record);
                }
                else
                {
                    response.ReturnCode = GrpcReturnCode.Error_201;
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "GetProduct", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetProductList
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.GetProductList_Response> GetProductList(String_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.GetProductList_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                var query = DB.Find<mdProduct>();
                if (!string.IsNullOrWhiteSpace(request.StringValue)) query.Match(x => x.VendorID == request.StringValue);
                //
                var gotRecords = await query.ExecuteAsync();
                //
                if (gotRecords != null)
                {
                    foreach (var item in gotRecords)
                    {
                        var retRecord = new grpcProductModel();
                        ClassHelper.CopyPropertiesData(item, retRecord);
                        //
                        response.Records.Add(retRecord);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "GetProductList", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // CreateSaleOrder
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.String_Response> CreateSaleOrder(CreateSaleOrder_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                mdSaleOrder saveRecord = new mdSaleOrder();
                //
                ClassHelper.CopyPropertiesData(request.Record, saveRecord);
                //
                saveRecord.ID = saveRecord.GenerateNewID();
                saveRecord.OrderID = await MyVoucher.GetVoucherNo("001");
                saveRecord.ModifiedOn = DateTime.Now;
                //
                await saveRecord.SaveAsync();
                //
                response.StringValue = saveRecord.OrderID;
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "CreateSaleOrder", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }




    }//End class
}//End name space
