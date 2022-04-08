using Cores.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Common
{
    public static class MyDateTimeSelector
    {
        public static List<CodeNameModel> Get_TimeRangeList()
        {
            return new List<CodeNameModel>() {
                new CodeNameModel{CodeInt = 1, Name="Tuần này"},
                new CodeNameModel{CodeInt = 2, Name="Tuần trước"},
                new CodeNameModel{CodeInt = 3, Name="Tháng này"},
                new CodeNameModel{CodeInt = 4, Name="Tháng trước"},
                new CodeNameModel{CodeInt = 5, Name="Năm nay"}
            };
        }
        public static DateTimeRangeModel Select_DateTimeRange1(CodeNameModel seletedItem)
        {
            var ret = new DateTimeRangeModel();
            //Clear
            if (seletedItem == null)
            {
                ret.StartDate = DateTime.Today.MinDate();
                ret.EndDate = DateTime.Today.MaxDate();
                return ret;
            }
            
            //Tuan nay
            if (seletedItem.CodeInt == 1)
            {
                ret.StartDate = DateTime.Today.FirstDayOfWeek();
                ret.EndDate = DateTime.Today.LastDayOfWeek();
            }
            //Tuan truoc
            if (seletedItem.CodeInt == 2)
            {
                ret.StartDate = DateTime.Today.AddDays(-7).FirstDayOfWeek();
                ret.EndDate = DateTime.Today.AddDays(-7).LastDayOfWeek();
            }
            //Thang nay
            if (seletedItem.CodeInt == 3)
            {
                ret.StartDate = DateTime.Today.FirstDayOfMonth();
                ret.EndDate = DateTime.Today.LastDayOfMonth();
            }
            //Thang truoc
            if (seletedItem.CodeInt == 4)
            {
                ret.StartDate = DateTime.Today.AddMonths(-1).FirstDayOfMonth();
                ret.EndDate = DateTime.Today.AddMonths(-1).LastDayOfMonth();
            }
            //Nam nay
            if (seletedItem.CodeInt == 5)
            {
                ret.StartDate = DateTime.Today.FirstDayOfYear();
                ret.EndDate = DateTime.Today.LastDayOfYear();
            }
            //
            return ret;
        }


    }// end class

    public class DateTimeRangeModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
