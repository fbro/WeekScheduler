using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public class ActivityRecordModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 0)]
        [Display(Name = "Employee initials")]
        public string EmployeeID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 1)]
        [Display(Name = "Activity ID")]
        public int ActivityID { get; set; }

        [Required]
        [Display(Name = "Activity Weight")]
        public int Weight { get; set; } = 0;
    }
}
