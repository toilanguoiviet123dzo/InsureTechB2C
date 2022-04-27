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
                                             .ExecuteFirstAsync();
                    }
                    if (saveRecord != null)
                    {
                        ClassHelper.CopyPropertiesData(request.Record, saveRecord);

                        //Specifications
                        saveRecord.Specifications.Clear();
                        if (request.Record.Specifications != null)
                        {
                            foreach (var item in request.Record.Specifications)
                            {
                                var specItem = new SpecificationModel();
                                ClassHelper.CopyPropertiesData(item, specItem);
                                //
                                saveRecord.Specifications.Add(specItem);
                            }
                        }

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
                                    .ExecuteFirstAsync();

                if (gotRecord != null)
                {
                    response.Record = new grpcProductModel();
                    ClassHelper.CopyPropertiesData(gotRecord, response.Record);

                    //Specifications
                    response.Record.Specifications.Clear();
                    if (gotRecord.Specifications != null)
                    {
                        foreach (var item in gotRecord.Specifications)
                        {
                            var specItem = new grpcSpecificationModel();
                            ClassHelper.CopyPropertiesData(item, specItem);
                            //
                            response.Record.Specifications.Add(specItem);
                        }
                    }
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

                        //Specifications
                        retRecord.Specifications.Clear();
                        if (item.Specifications != null)
                        {
                            foreach (var specItem in item.Specifications)
                            {
                                var grpcSpecItem = new grpcSpecificationModel();
                                ClassHelper.CopyPropertiesData(specItem, grpcSpecItem);
                                //
                                retRecord.Specifications.Add(grpcSpecItem);
                            }
                        }
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
        // DuplicatedCheck
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.Empty_Response> DuplicatedCheck(DuplicatedCheck_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.Empty_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                //Duplicated check
                var record = await DB.Find<mdSaleOrderLog>()
                                     .Match(x => request.EffectiveSttDate.ToDateTime() < x.EffectiveEndDate)
                                     .Match(x => request.ProductID == x.ProductID)
                                     .Match(x => request.LicensePlate == x.LicensePlate)
                                     .ExecuteFirstAsync();
                if (record != null)
                {
                    response.ReturnCode = GrpcReturnCode.Error_201;
                    response.MsgCode = "Bị trùng lập !!!";
                    response.MsgCode += $"Bạn đã mua trước đây có kỳ hạn: {record.EffectiveSttDate.ToLocalTime().ToString("dd/MM/yyyy")} ~ {record.EffectiveEndDate.ToLocalTime().ToString("dd/MM/yyyy")}";
                    response.MsgCode += $"Xin hãy chọn lại kỳ hạn mới";
                    //
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "InitOrder", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // InitOrder
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.InitOrder_Response> InitOrder(InitOrder_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.InitOrder_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                //Create new order
                mdSaleOrder saveRecord = new mdSaleOrder();
                //
                ClassHelper.CopyPropertiesData(request.Record, saveRecord);
                //
                saveRecord.ID = saveRecord.GenerateNewID();
                saveRecord.TransactionID = MyCodeGenerator.GenTransactionID();
                //Time
                saveRecord.RequestTime = DateTime.UtcNow;
                saveRecord.ModifiedOn = DateTime.Now;
                saveRecord.UpdMode = 1;

                //RequestTime, ExpiredTime
                int timeOutIn = await SettingMaster.GetInt1("017");
                if (timeOutIn == 0) timeOutIn = 30;
                //
                saveRecord.RequestTime = DateTime.UtcNow;
                saveRecord.ExpiredTime = DateTime.UtcNow.AddMinutes(timeOutIn);
                //
                await saveRecord.SaveAsync();
                //
                response.InitOrderToken = MyTokenService.GenInitOrderToken(saveRecord.TransactionID);
                response.TransactionID = saveRecord.TransactionID;
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "InitOrder", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // CheckPayStatus
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.CheckPayStatus_Response> CheckPayStatus(String_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.CheckPayStatus_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                var gotRecord = await DB.Find<mdSaleOrder>()
                                        .Match(x => x.TransactionID == request.StringValue)
                                        .ExecuteFirstAsync();

                if (gotRecord != null)
                {
                    response.IsPayDone = gotRecord.IsPayDone;
                    response.IsPayError = gotRecord.IsPayError;
                }
                else
                {
                    response.IsPayError = true;
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "CheckPayStatus", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetCertificateList
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.GetCertificateList_Response> GetCertificateList(GetCertificateList_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.GetCertificateList_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                var gotRecords = await DB.Find<mdSaleOrderLog>()
                                         .Match(x => x.CusPhone == request.CusPhone)
                                         .Match(x => x.CusCitizenID == request.CusCitizenID)
                                         .ExecuteAsync();
                //
                if (gotRecords != null)
                {
                    foreach (var item in gotRecords)
                    {
                        var retRecord = new grpcSaleOrderModel();
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "GetCertificateList", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetSaleOrderByPhone
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.GetSaleOrderByPhone_Response> GetSaleOrderByPhone(GetSaleOrderByPhone_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.GetSaleOrderByPhone_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                //By Phone, ProductID
                var record = await DB.Find<mdSaleOrderLog>()
                                         .Match(x => x.CusPhone == request.CusPhone)
                                         .Match(x => x.ProductID == request.ProductID)
                                         .Sort(x => x.TransactionID, Order.Descending)
                                         .ExecuteFirstAsync();
                //
                if (record != null)
                {
                    response.IsMatchByProduct = true;
                    response.Record = new grpcSaleOrderModel();
                    ClassHelper.CopyPropertiesData(record, response.Record);
                }

                //By Phone
                record = await DB.Find<mdSaleOrderLog>()
                                         .Match(x => x.CusPhone == request.CusPhone)
                                         .Sort(x => x.TransactionID, Order.Descending)
                                         .ExecuteFirstAsync();
                //
                if (record != null)
                {
                    response.Record = new grpcSaleOrderModel();
                    ClassHelper.CopyPropertiesData(record, response.Record);
                }
                //Not found
                if (record == null) response.ReturnCode = GrpcReturnCode.Error_201;
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "GetSaleOrderByPhone", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetUserDiscountCodeList
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.GetUserDiscountCodeList_Response> GetUserDiscountCodeList(String_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.GetUserDiscountCodeList_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                var records = await DB.Find<mdDiscountCode>()
                                         .Match(x => x.FromDate <= DateTime.UtcNow)
                                         .Match(x => x.ToDate >= DateTime.UtcNow)
                                         .Match(x => x.ProductID == request.StringValue)
                                         .Match(x => x.IsPublic == true)
                                         .Sort(x => x.DiscountRate, Order.Descending)
                                         .ExecuteAsync();
                //
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        if ((record.TotalMaxQty - record.UsedQty > 0) || (record.TotalMaxQty == 0))
                        {
                            var grpcItem = new grpcDiscountCodeModel();
                            ClassHelper.CopyPropertiesData(record, grpcItem);
                            //
                            response.Records.Add(grpcItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "GetUserDiscountCodeList", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // CheckDiscountCode
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Insure.Services.CheckDiscountCode_Response> CheckDiscountCode(String_Request request, ServerCallContext context)
        {
            var response = new Insure.Services.CheckDiscountCode_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                var record = await DB.Find<mdDiscountCode>()
                                         .Match(x => x.FromDate <= DateTime.UtcNow)
                                         .Match(x => x.ToDate >= DateTime.UtcNow)
                                         .Match(x => x.DiscountCode == request.StringValue)
                                         .ExecuteFirstAsync();
                //
                if (record != null && ((record.TotalMaxQty - record.UsedQty > 0) || (record.TotalMaxQty == 0)))
                {
                    response.Record = new grpcDiscountCodeModel();
                    ClassHelper.CopyPropertiesData(record, response.Record);
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "InsureService", "CheckDiscountCode", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }





    }//End class
}//End name space
