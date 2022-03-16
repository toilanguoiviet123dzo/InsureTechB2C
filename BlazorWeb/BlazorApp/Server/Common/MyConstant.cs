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
        //Currency
        public const string NaturalCurrencyUnit = "VND";

        //Search
        public const int Search_MaxRecordCount = 1000;



    }// end class
}// end name space
