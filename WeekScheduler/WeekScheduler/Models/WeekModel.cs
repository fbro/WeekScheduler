using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public class WeekModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 0)]
        [Display(Name = "Project ID")]
        public int ProjectID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 1)]
        [RegularExpression(@"\d\d-\d\d\d\d", ErrorMessage = "week number first must be 2 decimals then a hyphen followed by a year of 4 decimals")]
        [Display(Name = "Week number and year")]
        public string WeekID { get; set; } = GetIso8601WeekOfYear(DateTime.Now) + "-" + DateTime.Now.Year;

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        [Display(Name = "Monday Date")]
        public DateTime FirstDayOfTheWeek { get; set; }

        [Display(Name = "Monday Notes")]
        public string Notes1Monday { get; set; } = "";

        [Display(Name = "Tuesday Notes")]
        public string Notes2Tuesday { get; set; } = "";

        [Display(Name = "Wednesday Notes")]
        public string Notes3Wednesday { get; set; } = "";

        [Display(Name = "Thursday Notes")]
        public string Notes4Thursday { get; set; } = "";

        [Display(Name = "Friday Notes")]
        public string Notes5Friday { get; set; } = "";

        [Display(Name = "Saturday Notes")]
        public string Notes6Saturday { get; set; } = "";

        [Display(Name = "Sunday Notes")]
        public string Notes7Sunday { get; set; } = "";

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static string GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            var weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string weekNumberString;
            if (weekNumber < 10)
                weekNumberString = "0" + weekNumber;
            else
                weekNumberString = "" + weekNumber;
            return weekNumberString;
        }

        public static DateTime GetMondayFromWeekID(string weekID)
        {
            int weekNumber = Int32.Parse(weekID.Substring(0, 2));
            int year = Int32.Parse(weekID.Substring(3, 4));
            DateTime time = new DateTime(year, 1, 1).AddDays(weekNumber * 7 - 6);
            time = time.AddDays(-((int)time.DayOfWeek - 1));
            return time;
        }
    }

    
}
