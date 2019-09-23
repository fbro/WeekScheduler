using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Data;

namespace WeekScheduler.ViewModels
{
    public class IndexAdminViewModel
    {
        [Display(Name = "users")]
        public List<ApplicationUser> Users { get; set; }

        [Display(Name = "roles")]
        public List<Microsoft.AspNetCore.Identity.IdentityRole> Roles { get; set; }

        [Display(Name = "user roles")]
        public List<Microsoft.AspNetCore.Identity.IdentityUserRole<string>> UserRoles { get; set; }

        public string Message { get; set; }
        public bool? IsDBResetConfirmed { get; set; }
    }
}
