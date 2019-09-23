using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public class EmployeeProjectModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 0)]
        [Display(Name = "Project ID")]
        public int ProjectID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 1)]
        [Display(Name = "Employee initials")]
        public string EmployeeID { get; set; }
    }
}
