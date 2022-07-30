using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Services;
using Insure.Services;
using Resource.Services;
using Cores.GrpcClient.Authentication;
using BlazorApp.Client.BindingModels;
using Cores.Helpers;
using Cores.Grpc.Client;

namespace BlazorApp.Client.Services
{
    public class MasterService
    {
        private readonly grpcAdminService.grpcAdminServiceClient _adminServiceClient;
        private readonly grpcInsureService.grpcInsureServiceClient _InsureServicesClient;
        private readonly grpcResourceService.grpcResourceServiceClient _resourceServicesClient;

        public MasterService(grpcAdminService.grpcAdminServiceClient adminServiceClient,
                             grpcInsureService.grpcInsureServiceClient InsureServicesClient,
                             grpcResourceService.grpcResourceServiceClient ResourceServicesClient)
        {
            _adminServiceClient = adminServiceClient;
            _InsureServicesClient = InsureServicesClient;
            _resourceServicesClient = ResourceServicesClient;
        }
        //OptionLists
        private List<OptionListModel> OptionLists = new List<OptionListModel>();
        public async Task<List<OptionListModel>> Load_OptionList(string ListCode)
        {
            //Get from DB
            if (OptionLists.Count == 0)
            {
                try
                {
                    var request = new Admin.Services.String_Request()
                    {
                        Credential = new Admin.Services.UserCredential()
                        {
                            Username = WebUserCredential.Username,
                            RoleID = WebUserCredential.RoleID,
                            AccessToken = WebUserCredential.AccessToken,
                            ApiKey = WebUserCredential.ApiKey
                        },
                        StringValue = ""

                    };
                    //Get data from DB
                    var response = await _adminServiceClient.GetOptionListAsync(request);
                    if (response != null && response.ReturnCode == 200)
                    {
                        foreach (var item in response.OptionList)
                        {
                            var dataRow = new OptionListModel();
                            ClassHelper.CopyPropertiesDataDateConverted(item, dataRow);
                            OptionLists.Add(dataRow);
                        }
                    }

                }
                catch { }
            }
            //Ger list by ListCode
            var result = new List<OptionListModel>();
            foreach (var record in OptionLists)
            {
                if (record.ListCode == ListCode)
                {
                    var dataRow = new OptionListModel();
                    ClassHelper.CopyPropertiesDataDateConverted(record, dataRow);
                    result.Add(dataRow);
                }
            }
            //Order
            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.DspOrder).ToList<OptionListModel>();
            }
            //
            return result;
        }

        //UserList
        private List<UserAccountModel> UserLists = new List<UserAccountModel>();
        public async Task<List<UserAccountModel>> Load_UserList()
        {
            try
            {
                if (UserLists.Count == 0)
                {
                    var request = new Admin.Services.String_Request();
                    request.Credential = new Admin.Services.UserCredential()
                    {
                        Username = WebUserCredential.Username,
                        RoleID = WebUserCredential.RoleID,
                        AccessToken = WebUserCredential.AccessToken,
                        ApiKey = WebUserCredential.ApiKey
                    };
                    //
                    var response = await _adminServiceClient.GetUserAccountAsync(request);
                    if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                    {
                        foreach (var item in response.UserAccounts)
                        {
                            //Parrent grid
                            var dataRow = new UserAccountModel();
                            ClassHelper.CopyPropertiesDataDateConverted(item, dataRow);
                            //
                            UserLists.Add(dataRow);
                        }
                    }
                    //Order
                    if (UserLists.Count > 0)
                    {
                        UserLists = UserLists.OrderBy(x => x.Fullname).ToList<UserAccountModel>();
                    }
                }
            }
            catch { }
            //
            return UserLists;
        }

        //Address List
        private List<CityModel> CityList = new List<CityModel>();
        private async Task Load_FullAdressList()
        {
            CityList.Clear();
            //
            var request = new Admin.Services.Empty_Request();
            request.Credential = new Admin.Services.UserCredential()
            {
                Username = WebUserCredential.Username,
                RoleID = WebUserCredential.RoleID,
                AccessToken = WebUserCredential.AccessToken,
                ApiKey = WebUserCredential.ApiKey
            };
            //
            var response = await _adminServiceClient.GetFullAddressListAsync(request);
            if (response != null && response.ReturnCode == GrpcReturnCode.OK)
            {
                foreach (var city in response.Citys)
                {
                    //
                    var addCity = new CityModel();
                    ClassHelper.CopyPropertiesDataDateConverted(city, addCity);
                    //Districts
                    if (city != null)
                    {
                        foreach (var district in city.Districts)
                        {
                            var addDistrict = new DistrictModel();
                            ClassHelper.CopyPropertiesDataDateConverted(district, addDistrict);
                            //Wards
                            if (district.Wards != null)
                            {
                                foreach (var ward in district.Wards)
                                {
                                    var addWard = new WardModel();
                                    ClassHelper.CopyPropertiesDataDateConverted(ward, addWard);
                                    //
                                    addDistrict.Wards.Add(addWard);
                                }
                            }
                            //
                            addCity.Districts.Add(addDistrict);
                        }
                    }
                    //
                    CityList.Add(addCity);
                }
            }
        }
        //City List
        public async Task<List<AddressModel>> Load_CityList()
        {
            List<AddressModel> retList = new List<AddressModel>();
            //
            try
            {
                //Load full address list
                if (CityList.Count == 0)
                {
                    await Load_FullAdressList();
                }

                //Get city
                foreach (var city in CityList)
                {
                    var addressItem = new AddressModel();
                    addressItem.ID = city.ID;
                    addressItem.DspOrder = city.DspOrder;
                    addressItem.Level = 1;
                    addressItem.CityID = "";
                    addressItem.DistrictID = "";
                    addressItem.ItemID = city.CityID;
                    addressItem.ItemName = city.CityName;
                    addressItem.ItemNameEN = city.CityNameEN;
                    //
                    retList.Add(addressItem);
                }
                //Order
                if (retList.Count > 0)
                {
                    retList = retList.OrderBy(x => x.DspOrder).ToList();
                }
            }
            catch { }
            //
            return retList;
        }
        //District List
        public async Task<List<AddressModel>> Load_DistrictList(string cityID)
        {
            List<AddressModel> retList = new List<AddressModel>();
            //
            try
            {
                //Load full address list
                if (CityList.Count == 0)
                {
                    await Load_FullAdressList();
                }

                //Get city
                var city = CityList.Find(x => x.CityID == cityID);
                if (city != null && city.Districts != null)
                {
                    //Get District list
                    foreach (var district in city.Districts)
                    {
                        var addressItem = new AddressModel();
                        addressItem.ID = "";
                        addressItem.DspOrder = district.DspOrder;
                        addressItem.Level = 2;
                        addressItem.CityID = cityID;
                        addressItem.DistrictID = "";
                        addressItem.ItemID = district.DistrictID;
                        addressItem.ItemName = district.DistrictName;
                        addressItem.ItemNameEN = district.DistrictNameEN;
                        //
                        retList.Add(addressItem);
                    }
                    //Order
                    if (retList.Count > 0)
                    {
                        retList = retList.OrderBy(x => x.DspOrder).ToList();
                    }
                }
            }
            catch { }
            //
            return retList;
        }

        //District List
        public async Task<List<AddressModel>> Load_WardList(string cityID, string districtID)
        {
            List<AddressModel> retList = new List<AddressModel>();
            //
            try
            {
                //Load full address list
                if (CityList.Count == 0)
                {
                    await Load_FullAdressList();
                }

                //Get city
                var city = CityList.Find(x => x.CityID == cityID);
                if (city != null && city.Districts != null)
                {
                    //Get District
                    var district = city.Districts.Find(x => x.DistrictID == districtID);
                    if (district != null)
                    {
                        //Get Ward list
                        foreach (var ward in district.Wards)
                        {
                            var addressItem = new AddressModel();
                            addressItem.ID = "";
                            addressItem.DspOrder = ward.DspOrder;
                            addressItem.Level = 3;
                            addressItem.CityID = cityID;
                            addressItem.DistrictID = districtID;
                            addressItem.ItemID = ward.WardID;
                            addressItem.ItemName = ward.WardName;
                            addressItem.ItemNameEN = ward.WardNameEN;
                            //
                            retList.Add(addressItem);
                        }
                        //Order
                        if (retList.Count > 0)
                        {
                            retList = retList.OrderBy(x => x.DspOrder).ToList();
                        }
                    }
                }
            }
            catch { }
            //
            return retList;
        }

        //GetThumbnail
        private List<ThumbnailModel> ThumbnailList = new List<ThumbnailModel>();
        public async Task<byte[]> GetThumbnail(string resourceID)
        {
            byte[] retThumbnail = new byte[0];
            if (string.IsNullOrWhiteSpace(resourceID)) return retThumbnail;
            //
            try
            {
                //From cached
                var cacheItem = ThumbnailList.Find(x => x.ResourceID == resourceID);
                if (cacheItem != null) return cacheItem.Thumbnail;

                //Load image from DB
                var request = new Resource.Services.GetResourceFile_Request();
                var Credential = new Resource.Services.UserCredential()
                {
                    Username = WebUserCredential.Username,
                    RoleID = WebUserCredential.RoleID,
                    AccessToken = WebUserCredential.AccessToken,
                    ApiKey = WebUserCredential.ApiKey
                };
                request.Credential = Credential;
                //ClaimNo
                request.ResourceID = resourceID;
                request.IsGetThumbnail = true;
                //Call api
                var response = await _resourceServicesClient.GetResourceFileAsync(request);
                // Success
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    retThumbnail = response.Record.Thumbnail.ToByteArray();
                }
            }
            catch { }
            //
            return retThumbnail;
        }

        //GetImage
        private List<ResourceModel> ImageList = new List<ResourceModel>();
        public async Task<byte[]> GetImage(string resourceID)
        {
            byte[] retImage = new byte[0];
            if (string.IsNullOrWhiteSpace(resourceID)) return retImage;
            //
            try
            {
                //From cached
                var cacheItem = ImageList.Find(x => x.ResourceID == resourceID);
                if (cacheItem != null) return cacheItem.Content;

                //Load image from DB
                var request = new Resource.Services.GetResourceFile_Request();
                var Credential = new Resource.Services.UserCredential()
                {
                    Username = WebUserCredential.Username,
                    RoleID = WebUserCredential.RoleID,
                    AccessToken = WebUserCredential.AccessToken,
                    ApiKey = WebUserCredential.ApiKey
                };
                request.Credential = Credential;
                //ClaimNo
                request.ResourceID = resourceID;
                request.IsGetFull = true;
                //Call api
                var response = await _resourceServicesClient.GetResourceFileAsync(request);
                // Success
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    retImage = response.Record.FileContent.ToByteArray();
                }
            }
            catch { }
            //
            return retImage;
        }

        private byte[] BlankImage = new byte[0];
        public async Task<byte[]> GetBlankImage()
        {
            //from cached
            if (BlankImage.Length > 0) return BlankImage;
            //
            try
            {
                //Load image from DB
                var resourceID = "220721143130689560";
                var request = new Resource.Services.GetResourceFile_Request();
                var Credential = new Resource.Services.UserCredential()
                {
                    Username = WebUserCredential.Username,
                    RoleID = WebUserCredential.RoleID,
                    AccessToken = WebUserCredential.AccessToken,
                    ApiKey = WebUserCredential.ApiKey
                };
                request.Credential = Credential;
                //ClaimNo
                request.ResourceID = resourceID;
                request.IsGetThumbnail = true;
                //Call api
                var response = await _resourceServicesClient.GetResourceFileAsync(request);
                // Success
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    BlankImage = response.Record.Thumbnail.ToByteArray();
                }
            }
            catch { }
            //
            return BlankImage;
        }

        public async Task<DiscountCodeModel> CheckDiscountCode(string discountCode)
        {
            DiscountCodeModel RetModel = null;
            if (string.IsNullOrWhiteSpace(discountCode)) return RetModel;
            //
            try
            {
                //
                var request = new Insure.Services.String_Request()
                {
                    Credential = new Insure.Services.UserCredential()
                    {
                        Username = WebUserCredential.Username,
                        RoleID = WebUserCredential.RoleID,
                        AccessToken = WebUserCredential.AccessToken,
                        ApiKey = WebUserCredential.ApiKey
                    },
                    StringValue = discountCode
                };

                //Get data from DB
                var response = await _InsureServicesClient.CheckDiscountCodeAsync(request);
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    RetModel = new DiscountCodeModel();
                    ClassHelper.CopyPropertiesDataDateConverted(response.Record, RetModel);
                }
            }
            catch { }
            //
            return RetModel;
        }

        public async Task<ProductModel?> Get_Product(string productID)
        {
            try
            {
                //
                if (string.IsNullOrWhiteSpace(productID)) return null;

                var requestString = new Insure.Services.String_Request()
                {
                    Credential = new Insure.Services.UserCredential()
                    {
                        Username = WebUserCredential.Username,
                        RoleID = WebUserCredential.RoleID,
                        AccessToken = WebUserCredential.AccessToken,
                        ApiKey = WebUserCredential.ApiKey
                    },
                    StringValue = productID
                };

                //Get data from DB
                var response = await _InsureServicesClient.GetProductAsync(requestString);
                if (response != null && response.ReturnCode == GrpcReturnCode.OK)
                {
                    var retModel = new ProductModel();
                    ClassHelper.CopyPropertiesDataDateConverted(response.Record, retModel);

                    //Specifications
                    if (response.Record.Specifications != null)
                    {
                        foreach (var item in response.Record.Specifications)
                        {
                            var specItem = new SpecificationModel();
                            ClassHelper.CopyPropertiesData(item, specItem);
                            //
                            retModel.Specifications.Add(specItem);
                        }
                    }
                    //
                    return retModel;
                }
            }
            catch { }
            //
            return null;
        }




    }
}


public class ThumbnailModel
{
    public string ResourceID { get; set; }
    public byte[] Thumbnail { get; set; }
}

public class ResourceModel
{
    public string ResourceID { get; set; }
    public byte[] Content { get; set; }
}