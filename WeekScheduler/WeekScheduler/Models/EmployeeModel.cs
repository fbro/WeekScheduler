using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public class EmployeeModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        [Display(Name = "Employee initials")]
        public string EmployeeID { get; set; }

        [Required]
        [Display(Name = "Employee name")]
        public string Name { get; set; }

        [Required]
        //[MaxLength(100, ErrorMessage = "Weekly work hours cannot be higher than 100.")]
        [Display(Name = "Weekly work hours")]
        public int WeeklyWorkHours { get; set; } = 37;

        [Required]
        //[MaxLength(5, ErrorMessage = "Number of weekly counseling cannot be higher than 5.")]
        [Display(Name = "Number of weekly counseling")]
        public int NumOfWeeklyCounseling { get; set; } = 0;

        public string Color { get; set; } // ARGB

    }
}
