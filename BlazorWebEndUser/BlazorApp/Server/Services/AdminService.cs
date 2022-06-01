using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Cores.Grpc.Client;
using Admin.Services;
using WebPush;
using BlazorApp.Shared.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Entities;
using BlazorApp.Server.Models;
using BlazorApp.Server.Common;
using Cores.Helpers;
using Cores.Utilities;
using System.Data;
using System.IO;
using System.Data.OleDb;
using Resource.Services;

namespace BlazorApp.Server.Services
{
    public class AdminService : grpcAdminService.grpcAdminServiceBase
    {
        private readonly ILogger<AdminService> _logger;

        public AdminService(ILogger<AdminService> logger)
        {
            _logger = logger;
        }
        //-------------------------------------------------------------------------------------------------------/
        // GrpcLogin
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<GrpcLogin_Response> GrpcLogin(Admin.Services.GrpcLogin_Request request, ServerCallContext context)
        {
            var response = new GrpcLogin_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                var findRecord = await DB.Find<mdUserAccount>()
                                         .Match(x => x.UserName == request.UserName && x.Password == request.Password)
                                         .ExecuteFirstAsync();
                //
                if (findRecord != null)
                {
                    response.UserName = findRecord.UserName;
                    response.Fullname = findRecord.Fullname;
                    response.RoleID = findRecord.RoleID;
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
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GrpcLogin", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // CheckForCreateAccount
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> CreateAccountReq(Admin.Services.CreateAccountReq_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                //Duplicated check
                var findRecord = await DB.Find<mdUserAccount>()
                                         .Match(x => x.UserName == request.UserName)
                                         .ExecuteFirstAsync();
                //
                if (findRecord != null)
                {
                    response.MsgCode = "Tài khoản đã tồn tại";
                    response.ReturnCode = GrpcReturnCode.Error_202;
                    return response;
                }

                //Create queue for activation
                var record = new mdUserAccountQueue();
                record.ActivationCode = MyCodeGenerator.GenActivationCode();
                record.UserName = request.UserName; 
                record.Password = request.Password; 
                record.Fullname = request.Fullname; 
                record.Phone = request.Phone; 
                record.Email = request.Email;
                record.ModifiedOn = DateTime.UtcNow;
                record.ActivationOn = DateTime.UtcNow;
                //
                await record.SaveAsync();
                //
                response.StringValue = record.ActivationCode;
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "CreateAccountReq", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // ActivateAccount
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.Empty_Response> ActivateAccount(Admin.Services.String_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.Empty_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                //Duplicated check
                var queueRec = await DB.Find<mdUserAccountQueue>()
                                         .Match(x => x.ActivationCode == request.StringValue)
                                         .Match(x => x.IsDone == false)
                                         .ExecuteFirstAsync();
                //
                if (queueRec == null)
                {
                    response.MsgCode = "Mã kích hoạt không tồn tại";
                    response.ReturnCode = GrpcReturnCode.Error_201;
                    return response;
                }
                queueRec.ActivationOn = DateTime.UtcNow;
                queueRec.IsDone = true;
                await queueRec.SaveAsync();

                //Create account
                var record = new mdUserAccount();
                ClassHelper.CopyPropertiesData(queueRec, record);
                await record.SaveAsync();
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "ActivateAccount", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // SaveOptionList
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> SaveOptionList(SaveOptionList_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;  //OK
            //
            try
            {
                if (request.Record.UpdMode != 3)
                {
                    //Insert || Update
                    var saveRecord = new mdOptionList();
                    ClassHelper.CopyPropertiesData(request.Record, saveRecord);
                    //Insert
                    if (request.Record.UpdMode == 1) saveRecord.ID = saveRecord.GenerateNewID();
                    //
                    await saveRecord.SaveAsync();
                    //return saved ID
                    response.StringValue = saveRecord.ID;
                }
                else
                {
                    //Delete
                    await DB.DeleteAsync<mdOptionList>(request.Record.ID);
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "SaveOptionList", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }
        //-------------------------------------------------------------------------------------------------------/
        // SaveOptionListHeader
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> SaveOptionListHeader(SaveOptionListHeader_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                if (request.Record.UpdMode != 3)
                {
                    //Insert || Update
                    var saveRecord = new mdOptionListHeader();
                    ClassHelper.CopyPropertiesData(request.Record, saveRecord);
                    //Insert
                    if (request.Record.UpdMode == 1) saveRecord.ID = saveRecord.GenerateNewID();
                    //
                    await saveRecord.SaveAsync();
                    //return saved ID
                    response.StringValue = saveRecord.ID;
                }
                else
                {
                    //Delete
                    await DB.DeleteAsync<mdOptionListHeader>(request.Record.ID);
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "SaveOptionListHeader", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }
        //-------------------------------------------------------------------------------------------------------/
        // GetOptionListHeader
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<GetOptionListHeader_Response> GetOptionListHeader(Admin.Services.Empty_Request request, ServerCallContext context)
        {
            var response = new GetOptionListHeader_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                var findRecords = await DB.Find<mdOptionListHeader>().ExecuteAsync();
                //
                if (findRecords != null)
                {
                    findRecords.ForEach(item =>
                    {
                        var grpcItem = new grpcOptionListHeader();
                        ClassHelper.CopyPropertiesData(item, grpcItem);
                        response.OptionListHeader.Add(grpcItem);
                    });
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GetOptionListHeader", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }
        //-------------------------------------------------------------------------------------------------------/
        // GetOptionList
        //-------------------------------------------------------------------------------------------------------/

        public override async Task<GetOptionList_Response> GetOptionList(Admin.Services.String_Request request, ServerCallContext context)
        {
            var response = new GetOptionList_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                var findRecords = new List<mdOptionList>();
                if (request.StringValue == "")
                {
                    findRecords = await DB.Find<mdOptionList>()
                        .ExecuteAsync();
                }
                else
                {
                    findRecords = await DB.Find<mdOptionList>()
                            .Match(a => a.ListCode == request.StringValue)
                            .ExecuteAsync();
                }
                //
                if (findRecords != null)
                {
                    findRecords.ForEach(item =>
                    {
                        var grpcItem = new grpcOptionList();
                        ClassHelper.CopyPropertiesData(item, grpcItem);
                        response.OptionList.Add(grpcItem);
                    });
                }

            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GetOptionList", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // SaveSettingMaster
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> SaveSettingMaster(SaveSettingMaster_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                response.StringValue = request.Record.ID;
                //
                switch (request.Record.UpdMode)
                {
                    //add new
                    case 1:
                        var newRecord = new mdSettingMaster();
                        ClassHelper.CopyPropertiesData(request.Record, newRecord);
                        newRecord.ID = newRecord.GenerateNewID();
                        newRecord.ModifiedOn = DateTime.UtcNow;
                        //
                        response.StringValue = newRecord.ID;
                        //
                        await newRecord.SaveAsync();
                        break;

                    //update
                    case 2:
                        var oldRecord = await DB.Find<mdSettingMaster>().OneAsync(request.Record.ID);
                        if (oldRecord != null)
                        {
                            ClassHelper.CopyPropertiesData(request.Record, oldRecord);
                            oldRecord.ModifiedOn = DateTime.UtcNow;
                            //
                            await oldRecord.SaveAsync();
                        }
                        else
                        {
                            response.ReturnCode = GrpcReturnCode.Error_201;
                        }
                        break;

                    //delete
                    case 3:
                        await DB.DeleteAsync<mdSettingMaster>(request.Record.ID);
                        break;
                    default:
                        response.ReturnCode = GrpcReturnCode.Error_BadRequest; //UpdMode = blank
                        break;
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "SaveSettingMaster", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetSettingMaster
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<GetSettingMaster_Response> GetSettingMaster(Admin.Services.Empty_Request request, ServerCallContext context)
        {
            var response = new GetSettingMaster_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                var findRecords = await DB.Find<mdSettingMaster>().ExecuteAsync();
                //
                if (findRecords != null)
                {
                    findRecords.ForEach(item =>
                    {
                        var grpcItem = new grpcSettingMasterModel();
                        ClassHelper.CopyPropertiesData(item, grpcItem);
                        response.Records.Add(grpcItem);
                    });
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GetSettingMaster", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // SaveUserAccount
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> SaveUserAccount(SaveUserAccount_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            try
            {
                if (request.Record.UpdMode != 3)
                {
                    //Get Save password
                    var oldRecord = await DB.Find<mdUserAccount>()
                                            .Match(x => x.ID == request.Record.ID)
                                            .ExecuteFirstAsync();
                    string oldEnctyptedPassWord = "";
                    if (oldRecord != null)
                    {
                        oldEnctyptedPassWord = oldRecord.Password;
                    }

                    //Insert || Update
                    var saveRecord = new mdUserAccount();
                    ClassHelper.CopyPropertiesData(request.Record, saveRecord);
                    //Insert
                    if (request.Record.UpdMode == 1) saveRecord.ID = saveRecord.GenerateNewID();
                    //Encrypt password
                    if (!string.IsNullOrWhiteSpace(saveRecord.Password))
                    {
                        saveRecord.Password = saveRecord.Password;
                    }
                    else
                    {
                        saveRecord.Password = oldEnctyptedPassWord;
                    }
                    //
                    await saveRecord.SaveAsync();
                    //return saved ID
                    response.StringValue = saveRecord.ID;
                }
                else
                {
                    //Delete
                    await DB.DeleteAsync<mdUserAccount>(request.Record.ID);
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "SaveUserAccount", "Exception", response.ReturnCode, ex.Message);
            }
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetUserAccount
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<GetUserAccount_Response> GetUserAccount(Admin.Services.String_Request request, ServerCallContext context)
        {
            var response = new GetUserAccount_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                var query = DB.Find<mdUserAccount>();
                if (!string.IsNullOrWhiteSpace(request.StringValue))
                {
                    query.Match(a => a.UserName == request.StringValue);
                }
                var findRecords = await query.ExecuteAsync();
                //
                if (findRecords != null && findRecords.Count > 0)
                {
                    findRecords.ForEach(item =>
                    {
                        var grpcItem = new grpcUserAccountModel();
                        ClassHelper.CopyPropertiesData(item, grpcItem);
                        //Clear password
                        grpcItem.Password = "";
                        //
                        response.UserAccounts.Add(grpcItem);
                    });
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GetUserAccount", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }
        //-------------------------------------------------------------------------------------------------------/
        // GetVoucherNo
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> GetVoucherNo(Admin.Services.String_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            string newVoucherNo = "";
            //
            try
            {
                newVoucherNo = await MyVoucher.GetVoucherNo(request.StringValue);
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GetVoucherNo", "Exception", response.ReturnCode, ex.Message);
            }
            //
            response.StringValue = newVoucherNo;
            return response;
        }
        //-------------------------------------------------------------------------------------------------------/
        // Commit VoucherNo
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> CommitVoucherNo(Admin.Services.CommitVoucherNo_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            string committedVoucherNo = request.VoucherNo;
            //
            try
            {
                committedVoucherNo = await MyVoucher.CommitVoucherNo(request.VoucherCode, request.VoucherNo);
                if (string.IsNullOrWhiteSpace(committedVoucherNo))
                {
                    response.ReturnCode = GrpcReturnCode.Error_201;
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "CommitVoucherNo", "Exception", response.ReturnCode, ex.Message);
            }
            //
            response.StringValue = committedVoucherNo;
            return await Task.FromResult(response);
        }
        //-------------------------------------------------------------------------------------------------------/
        // SubscribeToNotifications
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.Empty_Response> SubscribeToNotifications(SubscribeToNotifications_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.Empty_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                //Duplicated check
                var updateRecord = await DB.Find<mdNotificationSubcribe>()
                                          .Match(a => a.Url == request.Url)
                                          .Match(a => a.P256dh == request.P256Dh)
                                          .Match(a => a.Auth == request.Auth)
                                          .ExecuteFirstAsync();
                if (updateRecord != null)
                {
                    return response;
                }

                //Insert new
                var newRecord = new mdNotificationSubcribe();
                newRecord.NotificationSubscriptionId = request.NotificationSubscriptionId;
                newRecord.UserId = request.Credential.Username;
                newRecord.Url = request.Url;
                newRecord.P256dh = request.P256Dh;
                newRecord.Auth = request.Auth;
                newRecord.ModifiedOn = DateTime.UtcNow;
                newRecord.UpdMode = 1;
                //
                await newRecord.SaveAsync();
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "SubscribeToNotifications", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }
        //-------------------------------------------------------------------------------------------------------/
        // GetNotificationSubscribe
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<GetNotificationSubscribe_Response> GetNotificationSubscribe(Admin.Services.String_Request request, ServerCallContext context)
        {
            var response = new GetNotificationSubscribe_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                //Duplicated check
                var findRecords = await DB.Find<mdNotificationSubcribe>()
                                          .Match(a => a.UserId == request.StringValue)
                                          .ExecuteAsync();
                if (findRecords != null)
                {
                    foreach (var item in findRecords)
                    {
                        var grpcItem = new grpcNotificationSubcribeModel();
                        ClassHelper.CopyPropertiesData(item, grpcItem);
                        grpcItem.P256Dh = item.P256dh;
                        //
                        response.Records.Add(grpcItem);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "SubscribeToNotifications", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // SaveAddressMaster
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.String_Response> SaveAddressMaster(SaveAddressMaster_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.String_Response();
            response.ReturnCode = GrpcReturnCode.OK;  //OK
            //
            try
            {
                //ID
                response.StringValue = request.Record.ID;

                //City
                if (request.Record.Level == 1)
                {
                    //Addnew
                    if (request.Record.UpdMode == 1 || request.Record.UpdMode == 2)
                    {
                        var cityRecord = await DB.Find<mdAddressMaster>()
                                                 .Match(x => x.CityID == request.Record.ItemID)
                                                 .ExecuteSingleAsync();
                        if (cityRecord != null)
                        {
                            //Update
                            cityRecord.DspOrder = request.Record.DspOrder;
                            cityRecord.CityID = request.Record.ItemID;
                            cityRecord.CityName = request.Record.ItemName;
                            cityRecord.CityNameEN = request.Record.ItemNameEN;
                            //
                            await cityRecord.SaveAsync();
                        }
                        else
                        {
                            //Add new
                            cityRecord = new mdAddressMaster();
                            cityRecord.GenerateNewID();
                            cityRecord.DspOrder = request.Record.DspOrder;
                            cityRecord.CityID = request.Record.ItemID;
                            cityRecord.CityName = request.Record.ItemName;
                            cityRecord.CityNameEN = request.Record.ItemNameEN;
                            //
                            await cityRecord.SaveAsync();
                            //return saved ID
                            response.StringValue = cityRecord.ID;
                        }
                    }
                    //Delete
                    if (request.Record.UpdMode == 3)
                    {
                        await DB.DeleteAsync<mdAddressMaster>(request.Record.ID);
                    }
                }

                //District
                if (request.Record.Level == 2)
                {
                    var cityRecord = await DB.Find<mdAddressMaster>()
                                                 .Match(x => x.CityID == request.Record.CityID)
                                                 .ExecuteSingleAsync();
                    if (cityRecord != null)
                    {
                        var districtRecord = cityRecord.Districts.Find(x => x.DistrictID == request.Record.ItemID);
                        if (districtRecord != null)
                        {
                            //Update
                            districtRecord.DspOrder = request.Record.DspOrder;
                            districtRecord.DistrictID = request.Record.ItemID;
                            districtRecord.DistrictName = request.Record.ItemName;
                            districtRecord.DistrictNameEN = request.Record.ItemNameEN;

                            //Delete
                            if (request.Record.UpdMode == 3)
                            {
                                cityRecord.Districts.Remove(districtRecord);
                            }
                        }
                        else
                        {
                            //Add new
                            districtRecord = new DistrictModel();
                            districtRecord.DspOrder = request.Record.DspOrder;
                            districtRecord.DistrictID = request.Record.ItemID;
                            districtRecord.DistrictName = request.Record.ItemName;
                            districtRecord.DistrictNameEN = request.Record.ItemNameEN;
                            //
                            cityRecord.Districts.Add(districtRecord);
                        }
                        //Save City
                        await cityRecord.SaveAsync();
                    }
                }

                //Ward
                if (request.Record.Level == 3)
                {
                    //find City
                    var cityRecord = await DB.Find<mdAddressMaster>()
                                                 .Match(x => x.CityID == request.Record.CityID)
                                                 .ExecuteSingleAsync();
                    //find District
                    if (cityRecord != null)
                    {
                        var districtRecord = cityRecord.Districts.Find(x => x.DistrictID == request.Record.DistrictID);
                        //
                        if (districtRecord != null)
                        {
                            var wardRecord = districtRecord.Wards.Find(x => x.WardID == request.Record.ItemID);
                            if (wardRecord != null)
                            {
                                //Update
                                wardRecord.DspOrder = request.Record.DspOrder;
                                wardRecord.WardID = request.Record.ItemID;
                                wardRecord.WardName = request.Record.ItemName;
                                wardRecord.WardNameEN = request.Record.ItemNameEN;

                                //Delete
                                if (request.Record.UpdMode == 3)
                                {
                                    districtRecord.Wards.Remove(wardRecord);
                                }
                            }
                            else
                            {
                                //Add new
                                wardRecord = new WardModel();
                                wardRecord.DspOrder = request.Record.DspOrder;
                                wardRecord.WardID = request.Record.ItemID;
                                wardRecord.WardName = request.Record.ItemName;
                                wardRecord.WardNameEN = request.Record.ItemNameEN;
                                //
                                districtRecord.Wards.Add(wardRecord);
                            }
                            //Save City
                            await cityRecord.SaveAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "SaveAddressMaster", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetFullAddressList
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<GetFullAddressList_Response> GetFullAddressList(Admin.Services.Empty_Request request, ServerCallContext context)
        {
            var response = new GetFullAddressList_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                var records = await DB.Find<mdAddressMaster>()
                                          .ExecuteAsync();
                if (records != null)
                {
                    foreach (var city in records)
                    {
                        //City
                        var grpcCity = new grpcCityModel();
                        ClassHelper.CopyPropertiesData(city, grpcCity);
                        response.Citys.Add(grpcCity);

                        //District
                        foreach (var district in city.Districts)
                        {
                            var grpcDistrict = new grpcDistrictModel();
                            ClassHelper.CopyPropertiesData(district, grpcDistrict);
                            grpcCity.Districts.Add(grpcDistrict);

                            //Ward
                            foreach (var ward in district.Wards)
                            {
                                var grpcWard = new grpcWardModel();
                                ClassHelper.CopyPropertiesData(ward, grpcWard);
                                grpcDistrict.Wards.Add(grpcWard);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GetFullAddressList", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // GetAddressList
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<GetAddressList_Response> GetAddressList(Admin.Services.GetAddressList_Request request, ServerCallContext context)
        {
            var response = new GetAddressList_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                //City list
                if (request.Level == 1)
                {
                    var records = await DB.Find<mdAddressMaster>()
                                          .ExecuteAsync();
                    if (records != null)
                    {
                        foreach (var city in records)
                        {
                            //City
                            var grpcCity = new grpcAddressModel();
                            grpcCity.ID = city.ID;
                            grpcCity.DspOrder = city.DspOrder;
                            grpcCity.Level = 1;
                            grpcCity.ItemID = city.CityID;
                            grpcCity.ItemName = city.CityName;
                            grpcCity.ItemNameEN = city.CityNameEN;
                            response.Records.Add(grpcCity);
                        }
                    }
                }

                //District list
                if (request.Level == 2)
                {
                    var city = await DB.Find<mdAddressMaster>()
                                          .Match(x => x.CityID == request.CityID)
                                          .ExecuteSingleAsync();
                    if (city != null)
                    {
                        foreach (var district in city.Districts)
                        {
                            //City
                            var grpcDistrict = new grpcAddressModel();
                            grpcDistrict.DspOrder = district.DspOrder;
                            grpcDistrict.Level = 2;
                            grpcDistrict.CityID = city.CityID;
                            grpcDistrict.ItemID = district.DistrictID;
                            grpcDistrict.ItemName = district.DistrictName;
                            grpcDistrict.ItemNameEN = district.DistrictNameEN;
                            response.Records.Add(grpcDistrict);
                        }
                    }
                }

                //Ward list
                if (request.Level == 3)
                {
                    var city = await DB.Find<mdAddressMaster>()
                                          .Match(x => x.CityID == request.CityID)
                                          .ExecuteSingleAsync();
                    if (city != null && city.Districts.Count > 0)
                    {
                        var district = city.Districts.Find(x => x.DistrictID == request.DistrictID);
                        if (district != null)
                        {
                            foreach (var ward in district.Wards)
                            {
                                //City
                                var grpcWard = new grpcAddressModel();
                                grpcWard.DspOrder = ward.DspOrder;
                                grpcWard.Level = 3;
                                grpcWard.CityID = city.CityID;
                                grpcWard.DistrictID = district.DistrictID;
                                grpcWard.ItemID = ward.WardID;
                                grpcWard.ItemName = ward.WardName;
                                grpcWard.ItemNameEN = ward.WardNameEN;
                                response.Records.Add(grpcWard);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "GetCityList", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        //-------------------------------------------------------------------------------------------------------/
        // ImportAddressList
        //-------------------------------------------------------------------------------------------------------/
        public override async Task<Admin.Services.Empty_Response> ImportAddressList(Admin.Services.ImportAddressList_Request request, ServerCallContext context)
        {
            var response = new Admin.Services.Empty_Response();
            response.ReturnCode = GrpcReturnCode.OK;
            response.MsgCode = "";
            //
            try
            {
                //archiveFolder
                SettingMasterModel settingMaster;
                string archiveFolder = "";
                settingMaster = await SettingMaster.GetSetting("001");
                archiveFolder = settingMaster == null ? "" : settingMaster.StringValue1;

                //Get file name
                var resourceFile = await DB.Find<mdResourceFile>()
                                       .Match(x => x.ResourceID == request.ResourceID)
                                       .ExecuteSingleAsync();
                string fileName = "";
                if (resourceFile != null)
                {
                    fileName = archiveFolder + resourceFile.ServerFileName;
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    response.ReturnCode = GrpcReturnCode.Error_201;
                    return response;
                }

                //Read excel
                var excelData = ReadExcelFile(fileName);

                //Insert to DB
                if (excelData != null && excelData.Rows.Count > 0)
                {
                    int dspOrder = 9000;
                    foreach (DataRow row in excelData.Rows)
                    {
                        //City
                        if (request.Level == 1)
                        {
                            string itemID = row[0] == DBNull.Value ? "" : ((string)row[0]).Trim();
                            string itemName = row[1] == DBNull.Value ? "" : ((string)row[1]).Trim();
                            if (!string.IsNullOrWhiteSpace(itemID))
                            {
                                var saveRequest = new SaveAddressMaster_Request();
                                saveRequest.Record = new grpcAddressModel();
                                saveRequest.Record.DspOrder = dspOrder;
                                saveRequest.Record.Level = 1;
                                saveRequest.Record.CityID = "";
                                saveRequest.Record.DistrictID = "";
                                saveRequest.Record.ItemID = itemID;
                                saveRequest.Record.ItemName = itemName;
                                saveRequest.Record.ItemNameEN = "";
                                saveRequest.Record.UpdMode = 1;
                                //
                                var saveResponse = await SaveAddressMaster(saveRequest, context);
                                //
                                dspOrder += 1;
                            }
                        }
                        //District
                        if (request.Level == 2)
                        {
                            string itemID = row[0] == DBNull.Value ? "" : ((string)row[0]).Trim();
                            string itemName = row[1] == DBNull.Value ? "" : ((string)row[1]).Trim();
                            string cityID = row[4] == DBNull.Value ? "" : ((string)row[4]).Trim();
                            if (!string.IsNullOrWhiteSpace(itemID))
                            {
                                var saveRequest = new SaveAddressMaster_Request();
                                saveRequest.Record = new grpcAddressModel();
                                saveRequest.Record.DspOrder = dspOrder;
                                saveRequest.Record.Level = 2;
                                saveRequest.Record.CityID = cityID;
                                saveRequest.Record.DistrictID = "";
                                saveRequest.Record.ItemID = itemID;
                                saveRequest.Record.ItemName = itemName;
                                saveRequest.Record.ItemNameEN = "";
                                saveRequest.Record.UpdMode = 1;
                                //
                                var saveResponse = await SaveAddressMaster(saveRequest, context);
                                //
                                dspOrder += 1;
                            }
                        }
                        //Ward
                        if (request.Level == 3)
                        {
                            string itemID = row[0] == DBNull.Value ? "" : ((string)row[0]).Trim();
                            string itemName = row[1] == DBNull.Value ? "" : ((string)row[1]).Trim();
                            string districtID = row[4] == DBNull.Value ? "" : ((string)row[4]).Trim();
                            string cityID = row[6] == DBNull.Value ? "" : ((string)row[6]).Trim();
                            if (!string.IsNullOrWhiteSpace(itemID))
                            {
                                var saveRequest = new SaveAddressMaster_Request();
                                saveRequest.Record = new grpcAddressModel();
                                saveRequest.Record.DspOrder = dspOrder;
                                saveRequest.Record.Level = 3;
                                saveRequest.Record.CityID = cityID;
                                saveRequest.Record.DistrictID = districtID;
                                saveRequest.Record.ItemID = itemID;
                                saveRequest.Record.ItemName = itemName;
                                saveRequest.Record.ItemNameEN = "";
                                saveRequest.Record.UpdMode = 1;
                                //
                                var saveResponse = await SaveAddressMaster(saveRequest, context);
                                //
                                dspOrder += 1;
                            }
                        }
                    }
                }

                //Delete old file
                var deleteFile = new grpcResourceFileModel();
                deleteFile.ResourceID = request.ResourceID;
                deleteFile.UpdMode = 3;
                //
                var deleteResponse = await ResourceService.SaveResourceFile(deleteFile);
            }
            catch (Exception ex)
            {
                response.ReturnCode = GrpcReturnCode.Error_ByServer;
                response.MsgCode = ex.Message;
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "AdminService", "ImportAddressList", "Exception", response.ReturnCode, ex.Message);
            }
            //
            return await Task.FromResult(response);
        }

        private DataTable ReadExcelFile(string fileName)
        {
            string fileExt = MyFile.Get_FileExtention(fileName);
            string conn = string.Empty;
            DataTable dtexcel = new DataTable();

            conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=NO';"; //for above excel 2007  

            //if (fileExt.CompareTo("xls") == 0)
            //    conn = @"provider=microsoft.jet.oledb.4.0;data source=" + fileName + ";extended properties='excel 8.0;hrd=yes;imex=1';"; //for below excel 2007  
            //else
            //    conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=NO';"; //for above excel 2007  
            //
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1  
                    oleAdpt.Fill(dtexcel); //fill excel data into dataTable  
                }
                catch { }
            }
            //
            return dtexcel;

        }


    }//End class
}//End namespace
