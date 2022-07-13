using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazorApp.Server.Common
{
    public static class MyConstant
    {
        //Log level
        public const int LogLevel_Trace = 0;
        public const int LogLevel_Debug = 1;
        public const int LogLevel_Information = 2;
        public const int LogLevel_Warning = 3;
        public const int LogLevel_Error = 4;
        public const int LogLevel_Critical = 5;
        public const int LogLevel_None = 6;
        public const int LogLevel_ForSale = 7;
        
        //Currency
        public const string NaturalCurrencyUnit = "VND";

        //Search
        public const int Search_MaxRecordCount = 1000;

        //Http
        public const string HttpClient_Common = "HttpClient_Common";

        //Payment channel list
        public const string PaymentChannel_Cash = "Cash";
        public const string PaymentChannel_VnPay = "VnPay";

        //ProductType
        public const string ProductType_Motocycle = "Motorcycle";
        public const string ProductType_AutoMotor = "AutoMotor";
        public const string ProductType_Health = "Health";

        //Vendor
        public const string Vendor_BMI = "BMI";
        public const string Vendor_BHV = "BHV";

        //Product
        public const string Product_MotorBMI = "TNDS_BM";
        public const string Product_MotorBHV = "TNDS_HV";
        public const string Product_AutoBMI = "TNDS_BM02";
        public const string Product_AutoBHV = "TNDS_HV02";


    }// end class
}// end name space
