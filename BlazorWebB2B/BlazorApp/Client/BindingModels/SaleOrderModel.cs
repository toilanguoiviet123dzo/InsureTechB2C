using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class SaleOrderModel
    {
        //SO - header
        public string TransactionID { get; set; } = "";
        public string OrderID { get; set; } = "";
        public DateTime OrderDate { get; set; }

        //Merchant
        public string MerchantID { get; set; } = "";
        public string AccountID { get; set; } = "";
        public double BonusRate { get; set; }
        public double BonusAmount { get; set; }
        public bool IsPayBonus { get; set; } = false;

        //SO - amount
        public int Quantity { get; set; } = 1;
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public bool IsIncludeVAT { get; set; }
        public double TaxRate { get; set; }
        public double TaxAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public double ExchangeRate { get; set; } = 1;

        //Discount
        public string DiscountCode { get; set; } = "";
        public string DiscountName { get; set; } = "";
        public double DiscountRate { get; set; }
        public double DiscountAmount { get; set; }
        public double PaymentAmount { get; set; }

        //Customer
        public string CusFullname { get; set; } = "";
        public string CusPhone { get; set; } = "";
        public string CusEmail { get; set; } = "";
        public string CusCitizenID { get; set; } = "";
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
        public string VendorLogoID { get; set; } = "";
        public string VendorID { get; set; } = "";
        public string VendorName { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public byte[] LogoContent { get; set; } = new byte[] { };
        public byte[] VendorLogoContent { get; set; } = new byte[] { };
        public string SaleImageID { get; set; } = "";
        public byte[] SaleImageContent { get; set; } = new byte[] { };

        //Status
        public DateTime RequestTime { get; set; }
        public DateTime PaymentTime { get; set; }
        public DateTime ExpiredTime { get; set; }
        public bool IsPayRequest { get; set; }
        public bool IsPayDone { get; set; }
        public bool IsPayError { get; set; }
        public bool IsProcessDone { get; set; }
        public bool IsProcessError { get; set; }
        public bool IsIssueCertificate { get; set; }

        //Payment
        public string BuyPolicy { get; set; } = "";
        public string PaymentChannelID { get; set; } = "";
        public string PaymentRefID { get; set; } = "";
        public string PaymentResponseCode { get; set; } = "";
        public string PaymentResponseData { get; set; } = "";

        // Vehicle info
        public string LicensePlate { get; set; } = "";
        public string BusinessType { get; set; } = "";
        public string BusinessTypeName { get; set; } = "";
        public string CarType { get; set; } = "";
        public string CarTypeName { get; set; } = "";
        public double SeatCount { get; set; }
        public double Tonage { get; set; }
        public bool IsBySeat { get; set; } = true;
        public bool Motor2People { get; set; }
        public int BuyYear { get; set; } = 1;

        //Single holder
        public string PolicyNo { get; set; } = "";
        public string HolderID { get; set; } = "";
        public DateTime PolicyDate { get; set; }
        public string Fullname { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Sex { get; set; } = "";
        public string CitizenID { get; set; } = "";
        public string SalePackageID { get; set; } = "";
        public string SalePackageName { get; set; } = "";
        public string TargetID { get; set; } = "";
        public string TargetName { get; set; } = "";
        public DateTime EffectiveSttDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public int Duration { get; set; }
        public string DurationUnit { get; set; } = "";
        public double BenefitAmount { get; set; }
        public string CertificateLink { get; set; } = "";

        //Multiple holders
        public bool HasMultiple { get; set; }
        public List<PolicyModel> PolicyHolders { get; set; } = new List<PolicyModel>();

        //
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }

    public class PolicyModel
    {
        public string OrderID { get; set; } = "";
        public string PolicyNo { get; set; } = "";
        public DateTime PolicyDate { get; set; } = DateTime.Now;
        public string Fullname { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Sex { get; set; } = "";
        public string CusCitizenID { get; set; } = "";
        public string ProductType { get; set; } = "";
        public string VendorID { get; set; } = "";
        public string VendorName { get; set; } = "";
        public string ProductID { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string SalePackageID { get; set; } = "";
        public string SalePackageName { get; set; } = "";
        public string TargetID { get; set; } = "";
        public string TargetName { get; set; } = "";
        public DateTime EffectiveSttDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public int Duration { get; set; }
        public string DurationUnit { get; set; } = "";
        public double FeeAmount { get; set; }
        public double BenefitAmount { get; set; }
        public string CertificateLink { get; set; } = "";
    }

}
