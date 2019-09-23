using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeekScheduler.Data;
using WeekScheduler.ViewModels;
using Microsoft.Extensions.Configuration;

namespace WeekScheduler.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration Configuration)
        {
            _context = context;
            _configuration = Configuration;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string Message = "", bool? IsDBResetConfirmed = false)
        {
            var adminIndexViewModel = new IndexAdminViewModel()
            {
                Users = await _context.Users.ToListAsync(),
                Roles = await _context.Roles.ToListAsync(),
                UserRoles = await _context.UserRoles.ToListAsync(),
                Message = Message,
                IsDBResetConfirmed = IsDBResetConfirmed,
            };
            return View(adminIndexViewModel);
        }

        // POST: Home/TestInitialize
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult TestInitialize(bool isConfirmed = false)
        {
            if (isConfirmed)
            {
                var test = new Test.TestInitialize(_context);
                var isInitialized = test.TestInit();
                return RedirectToAction("Index", "Admin", new { Message = "RESET DATABASE: " + isInitialized });
            }
            else
            {
                return RedirectToAction("Index", "Admin", new { Message = "CLICK Reset Database AGAIN TO CONFIRM DATABASE RESET!", isDBResetConfirmed = true });
            }
            
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserRole(string EmployeeID, string RoleName)
        {
            var User = _context.Users.SingleOrDefault(e => e.Initials == EmployeeID);
            var UserId = User?.Id;
            var Role = _context.Roles.SingleOrDefault(e => e.Name.Equals(RoleName));
            var RoleId = Role?.Id;
            if (UserId == null || RoleId == null)
            {
                return NotFound();
            }
            foreach (var userRole in _context.UserRoles.Where(e => e.UserId == UserId))
            {
                if (userRole.RoleId == RoleId)
                {
                    return BadRequest(); // already exists
                }
            }
            var result = await _context.UserRoles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>() { UserId = UserId, RoleId = RoleId });
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin", new { Message = "AddUserRole: " + EmployeeID + ", RoleName: " + RoleName + ", User: " + User.ToString()});
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserRole(string EmployeeID, string RoleName)
        {
            var User = _context.Users.SingleOrDefault(e => e.Initials == EmployeeID);
            var UserId = User?.Id;
            var Role = _context.Roles.SingleOrDefault(e => e.Name.Equals(RoleName));
            var RoleId = Role?.Id;
            var UserRole = await _context.UserRoles.FindAsync(UserId, RoleId);
            if (UserId == null || RoleId == null || UserRole == null)
            {
                return NotFound();
            }
            var AdminUserEmail = _configuration.GetSection("AppSettings").GetValue<string>("AppSettings:AdminUserEmail");
            var result = _context.UserRoles.Remove(UserRole);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin", new { Message = "DeleteUserRole: " + EmployeeID + ", RoleName: " + RoleName + ", User: " + User.ToString()});
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetUserInitials(string Initials, string UserId)
        {
            if (Initials == null || UserId == null)
            {
                return NotFound();
            }
            var User = await _context.Users.FindAsync(UserId);
            if (User == null)
            {
                return BadRequest();
            }
            User.Initials = Initials;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin", new { Message = "SetUserInitials: " + Initials + ", UserId: " + UserId + ", User: " + User.ToString()});
        }
    }
}