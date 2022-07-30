using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Models;
using MongoDB.Entities;

namespace Server.Common
{
    public static class MyAppLog
    {
        public static async void WriteLog(int LogLevel,
                                    string Class,
                                    string Method,
                                    string Step,
                                    int ErrorCode,
                                    string ErrorMessage) 
        {
            try
            {
                var newRecord = new mdAppLog();
                newRecord.ID = "";
                newRecord.LogLevel = LogLevel;
                newRecord.Class = Class;
                newRecord.Method = Method;
                newRecord.Step = Step;
                newRecord.ErrorCode = ErrorCode;
                newRecord.ErrorMessage = ErrorMessage;
                newRecord.CreatedOn = DateTime.UtcNow;
                await newRecord.SaveAsync();
            }
            catch { }
        }
    }
}