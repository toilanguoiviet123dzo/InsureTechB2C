using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Common
{
    public static class MyConstant
    {
        //Grid
        public const int Grid_PageRowCount = 20;
        //Currency
        public const string NaturalCurrencyUnit = "VND";
        //Search
        public const int Search_MaxRecordCount = 1000;
        //Resource file
        public const int ResourceArchiveMode_InsideDB = 1;
        public const int ResourceArchiveMode_OutsideDB = 2;

        //MessageBoxIcon
        public const int MessageBoxIcon_Information = 1;
        public const int MessageBoxIcon_Question = 2;
        public const int MessageBoxIcon_Exclamation = 3;
        public const int MessageBoxIcon_Stop = 4;
        public const int MessageBoxIcon_Error = 5;

        //Action
        public const int ConfirmAction_DeleteRow = 1;

        //VendorID
        public const string Vendor_BMI = "BMI";
        public const string Vendor_BHV = "BHV";

        //Product type
        public const string ProductType_Motor = "Motorcycle";
        public const string ProductType_Auto = "AutoMotor";

        //Payment channel list
        public const string PaymentChannel_Cash = "Cash";
        public const string PaymentChannel_VnPay = "VnPay";


    }// end class
}
