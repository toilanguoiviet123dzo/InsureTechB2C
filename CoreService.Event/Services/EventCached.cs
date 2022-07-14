using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cores.Utilities;

namespace CoreService
{
    public static class EventCached
    {
        public static List<AppEventModel> AppEvents = new List<AppEventModel>();

        public static void AddOrUpdate(AppEventModel record)
        {
            if (AppEvents.Exists(x => x.ID == record.ID))
            {
                //update
                AppEvents.Replace(x => x.ID == record.ID, record);
            }
            else
            {
                //Add new
                AppEvents.Add(record);
            }
        }
        public static void Remove(string ID)
        {
            AppEvents.RemoveAll(x => x.ID == ID);
        }

        public static void RemoveAt(int index)
        {
            AppEvents.RemoveAt(index);
        }
    }

    public class AppEventModel
    {
        public string ID { get; set; } = "";
        public DateTime IssueDatetime { get; set; }
        public string Publicer { get; set; } = "";
        public string Subcriber { get; set; } = "";
        public string EventName { get; set; } = "";
        public string JsonStringData { get; set; } = "";
        public int CallType { get; set; }
        public bool CallDone { get; set; }
        public int MaxRetryCount { get; set; }
        public int RetryCount { get; set; }
        public bool ErrorFlag { get; set; }
        public string ErrorMessage { get; set; } = "";
        public bool NeedAlarm { get; set; }
        public bool StopAlarm { get; set; }
    }
}
