using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Data;
using WeekScheduler.Models;

namespace WeekScheduler.Controllers
{
    public class RulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rules
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rule.ToListAsync());
        }

        // POST: Rules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(RuleModel rule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Index", "Rules");
        }

        // POST: Rules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int RuleID)
        {
            var rule = await _context.Rule.FindAsync(RuleID);
            _context.Rule.Remove(rule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager")]
        public IActionResult ToggleRuleStatus(int? RuleID)
        {
            if (RuleID == null)
            {
                return NotFound();
            }
            var rule = _context.Rule.Single(e => e.RuleID == RuleID);
            if (rule.RuleStatus == true)
                rule.RuleStatus = false;
            else
                rule.RuleStatus = true;
            _context.Update(rule);
            _context.SaveChanges();
            return RedirectToAction("Index", "Rules");
        }
    }
}
