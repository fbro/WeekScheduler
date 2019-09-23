using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public class SlotModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 0)]
        [Display(Name = "Project ID")]
        public int ProjectID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 1)]
        [RegularExpression(@"\d\d-\d\d\d\d", ErrorMessage = "week number first must be 2 decimals then a hyphen followed by a year of 4 decimals")]
        [Display(Name = "Week number and year")]
        public string WeekID { get; set; } = WeekModel.GetIso8601WeekOfYear(DateTime.Now) + "-" + DateTime.Now.Year;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 2)]
        [Display(Name = "Activity ID")]
        public int ActivityID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 3)]
        [Display(Name = "Employee initials")]
        public string EmployeeID { get; set; }

        public string Notes { get; set; } = "";
    }
}
