#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HairdressersWebApplication1;
using Microsoft.AspNetCore.Authorization;

namespace HairdressersWebApplication1.Controllers
{
    [Authorize(Roles = "admin, user, worker")]
    public class BranchesController : Controller
    {
        private readonly HairdressersContext _context;

        public BranchesController(HairdressersContext context)
        {
            _context = context;
        }

        // GET: Branches
        public async Task<IActionResult> Index(string? f)
        {
            ViewBag.F = f;
            var branch = _context.Branches.ToListAsync();
            return View(await branch); 
        }

        // GET: Branches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(m => m.BranchId == id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // GET: Branches/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create(string? f)
        {
            ViewBag.F = f;
            return View();
        }

        // POST: Branches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BranchId,Address,Title,Phone")] Branch branch)
        {
            if (ModelState.IsValid && IsUnique(branch.Title, branch.Address))
            {
                _context.Add(branch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //return View(branch);
            return RedirectToAction("Create", "Branches", new { f = "Такий відділ вже існує" });
        }

        bool IsUnique(string title, string address)
        {
            var q = (from br in _context.Branches
                     where br.Title == title || br.Address == address
                     select br).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        // GET: Branches/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id, string? f)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.F = f;
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }
            return View(branch);
        }

        // POST: Branches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BranchId,Address,Title,Phone")] Branch branch)
        {
            if (id != branch.BranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid && IsUnique(branch.Title, branch.Address))
            {
                try
                {
                    _context.Update(branch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branch.BranchId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Edit", "Branches", new { f = "Такий відділ вже існує" });
        }

        // GET: Branches/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(m => m.BranchId == id);
            if (branch == null)
            {
                return NotFound();
            }
            var orders = await _context.Orders.FirstOrDefaultAsync(m => m.BranchId == id);
            if (orders != null)
            {
                return RedirectToAction("Index", "Branches", new {f = "В цьому відділі ще є замовлення"});
            }
            return View(branch);
        }

        // POST: Branches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.BranchId == id);
        }
    }
}
