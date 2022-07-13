using BlazorApp.Server.Models;
using Cores.Helpers;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Common
{
    public static class SettingMaster
    {
        public static List<SettingMasterModel> SettingMasters = new List<SettingMasterModel>();
        /// <summary>
        /// Get setting master
        /// </summary>
        /// <param name="settingCode"></param>
        /// <returns></returns>
        public async static Task<SettingMasterModel> GetSetting(string settingCode)
        {
            var ret = new SettingMasterModel();
            try
            {
                //Load for the first time
                if (SettingMasters.Count == 0)
                {
                    await Load_Setting();
                }

                //Get setting
                ret = SettingMasters.Find(x => x.Code == settingCode);
                return ret;
            }
            catch { }
            //
            return null;
        }
        /// <summary>
        /// Get string setting value
        /// </summary>
        /// <param name="settingCode"></param>
        /// <returns></returns>
        public async static Task<string> GetString1(string settingCode)
        {
            var ret = new SettingMasterModel();
            try
            {
                //Load for the first time
                if (SettingMasters.Count == 0)
                {
                    await Load_Setting();
                }

                //Get setting
                ret = SettingMasters.Find(x => x.Code == settingCode);
                return ret == null ? "" : ret.StringValue1;
            }
            catch { }
            //
            return "";
        }
        /// <summary>
        /// Get int setting value
        /// </summary>
        /// <param name="settingCode"></param>
        /// <returns></returns>
        public async static Task<int> GetInt1(string settingCode)
        {
            var ret = new SettingMasterModel();
            try
            {
                //Load for the first time
                if (SettingMasters.Count == 0)
                {
                    await Load_Setting();
                }

                //Get setting
                ret = SettingMasters.Find(x => x.Code == settingCode);
                return ret == null ? 0 : ret.IntValue1;
            }
            catch { }
            //
            return 0;
        }
        /// <summary>
        /// Get double setting value
        /// </summary>
        /// <param name="settingCode"></param>
        /// <returns></returns>
        public async static Task<double> GetDouble1(string settingCode)
        {
            var ret = new SettingMasterModel();
            try
            {
                //Load for the first time
                if (SettingMasters.Count == 0)
                {
                    await Load_Setting();
                }

                //Get setting
                ret = SettingMasters.Find(x => x.Code == settingCode);
                return ret == null ? 0.0 : ret.DoubleValue1;
            }
            catch { }
            //
            return 0.0;
        }
        //
        public async static Task Load_Setting()
        {
            try
            {
                var records = await DB.Find<mdSettingMaster>()
                                      .ExecuteAsync();

                //Result
                if (records != null)
                {
                    var temSettingMasters = new List<SettingMasterModel>();
                    //
                    foreach (var item in records)
                    {
                        var record = new SettingMasterModel();
                        ClassHelper.CopyPropertiesData(item, record);
                        temSettingMasters.Add(record);
                    }
                    //switch from temp
                    SettingMasters = temSettingMasters;
                }
            }
            catch { }
        }
        //
    }
}
