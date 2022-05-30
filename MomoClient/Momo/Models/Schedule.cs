using System;
using Xamarin.Forms;

namespace Momo.Models
{
    public class Schedule
    {
        // Data
        public string Id { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ColIdx { get; set; }
        public Common.EAlarmTimeType eType { get; set; }

        // UI
        public DateTime UITime { get; set; }
        public string FormatTT { get; set; }
        public string Time { get; set; }
        public bool Type1 { get; set; } // Only Start, Only End
        public bool Type2 { get; set; } // All Day
        public bool Type3 { get; set; } // Start and End of the day
        public string TypeEtc1 { get; set; }
        public string TypeEtc2 { get; set; }

        public void InitUI()
        {
            FormatTT = "";
            Time = "";
            Type1 = false;
            Type2 = false;
            Type3 = false;
            TypeEtc1 = "";
            TypeEtc2 = "";
        }
    }
}
