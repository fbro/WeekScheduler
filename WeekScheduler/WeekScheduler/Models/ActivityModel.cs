using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public class ActivityModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        [Display(Name = "Activity ID")]
        public int ActivityID { get; set; }

        [Required]
        [Display(Name = "Activity name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Day of week")]
        public DaySlot DayOfWeek { get; set; } = DaySlot.NOT_SET;

        [Required]
        [Display(Name = "Time of day")]
        public TimeSlot TimeOfDay { get; set; } = TimeSlot.NOT_SET;

        [Required]
        [Range(0, 24, ErrorMessage = "Work hours cannot be higher than 24.")]
        [Display(Name = "Work hours")]
        public double Hours { get; set; } = 0;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Activity importance min value is 1.")]
        [Display(Name = "Activity importance")]
        public int Importance { get; set; } = 1;

        [Display(Name = "Owner initials")]
        public string Owner { get; set; } = ""; // if this is set then the activity can only be assigned to the owner
    }
}
