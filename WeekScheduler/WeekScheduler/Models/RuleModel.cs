using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public class RuleModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        [Display(Name = "Rule ID")]
        public int RuleID { get; set; }

        [Required]
        [Display(Name = "Rule name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Rule status")]
        public bool RuleStatus { get; set; } = true;
    }
}
