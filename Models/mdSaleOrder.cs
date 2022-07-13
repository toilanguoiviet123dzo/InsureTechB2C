﻿using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("SaleOrder")]
    public class mdSaleOrder : Entity
    {
        //Order info
        public string TransactionID { get; set; } = "";
        public string OrderID { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public string PolicyID { get; set; } = "";
        public string QuoteNo { get; set; } = "";
        public string PolicyNo { get; set; } = "";
        public string OwnerFullname { get; set; } = "";
        public string LicensePlate { get; set; } = "";
        public string CusFullname { get; set; } = "";
        public string CusPhone { get; set; } = "";
        public string CusEmail { get; set; } = "";
        public string CusCitizenID { get; set; } = "";
        public DateTime EffectiveSttDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public string CityID { get; set; } = "";
        public string CityName { get; set; } = "";
        public string DistrictID { get; set; } = "";
        public string DistrictName { get; set; } = "";
        public string WardID { get; set; } = "";
        public string WardName { get; set; } = "";
        public string Address { get; set; } = "";
        public string PostalCode { get; set; } = "";
        //Product
        public string ProductType { get; set; } = "";
        public string VendorID { get; set; } = "";
        public string VendorName { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Duration { get; set; }
        public string DurationUnit { get; set; } = "";
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public bool IsIncludeVAT { get; set; } = true;
        public double TaxRate { get; set; }
        public double TaxAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public double ExchangeRate { get; set; } = 1;
        //For car
        public string BusinessType { get; set; } = "";
        public string CarType { get; set; } = "";
        public double SeatCount { get; set; }
        public double Tonage { get; set; }

        //For motor
        public bool Motor2People { get; set; }
        public int BuyYear { get; set; }

        //Discount
        public string DiscountCode { get; set; } = "";
        public double DiscountRate { get; set; }
        public double DiscountAmount { get; set; }
        public double PaymentAmount { get; set; }
        //Status
        public DateTime RequestTime { get; set; }
        public DateTime PaymentTime { get; set; }
        public DateTime ExpiredTime { get; set; }
        public bool IsPayRequest { get; set; }
        public bool IsPayDone { get; set; }
        public bool IsPayError { get; set; }
        public bool IsProcessDone { get; set; }
        public bool IsProcessError { get; set; }
        public string ProcessErrorMessage { get; set; } = "";
        public bool IsIssueCertificate { get; set; }
        public bool IsDelivery { get; set; }
        //Merchant
        public string MerchantID { get; set; } = "";
        public string AccountID { get; set; } = "";
        public double BonusRate { get; set; }
        public double BonusAmount { get; set; }
        public bool IsPayBonus { get; set; } = false;

        //Payment
        public string PaymentChannelID { get; set; } = "";
        public string PaymentRefID { get; set; } = "";
        public string PaymentResponseCode { get; set; } = "";
        public string PaymentResponseData { get; set; } = "";
        public string CertificateLink { get; set; } = "";
        //
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }



}
