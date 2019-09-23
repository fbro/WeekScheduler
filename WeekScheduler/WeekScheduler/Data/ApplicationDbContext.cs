using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WeekScheduler.Models;

namespace WeekScheduler.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /*
            delete Migrations folder 
            goto window: Package Manager Console
            Drop-Database
            Add-Migration InitialCreate
            Update-Database
        */
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ActivityModel> Activity { get; set; }

        public DbSet<ActivityRecordModel> ActivityRecord { get; set; }

        public DbSet<EmployeeModel> Employee { get; set; }

        public DbSet<EmployeeProjectModel> EmployeeProject { get; set; }

        public DbSet<ProjectModel> Project { get; set; }

        public DbSet<RuleModel> Rule { get; set; }

        public DbSet<SlotModel> Slot { get; set; }

        public DbSet<TagModel> Tag { get; set; }

        public DbSet<WeekModel> Week { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // declare composite keys
            modelBuilder.Entity<ActivityRecordModel>().HasKey(e => new { e.EmployeeID, e.ActivityID });
            modelBuilder.Entity<SlotModel>().HasKey(e => new { e.ProjectID, e.WeekID, e.ActivityID, e.EmployeeID });
            modelBuilder.Entity<WeekModel>().HasKey(e => new { e.ProjectID, e.WeekID });
            modelBuilder.Entity<EmployeeProjectModel>().HasKey(e => new { e.ProjectID, e.EmployeeID });
            modelBuilder.Entity<TagModel>().HasKey(e => new { e.TagID, e.ActivityID });
        }
    }
}