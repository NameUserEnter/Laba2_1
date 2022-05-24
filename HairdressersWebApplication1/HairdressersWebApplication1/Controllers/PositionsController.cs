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
    public class PositionsController : Controller
    {
        private readonly HairdressersContext _context;

        public PositionsController(HairdressersContext context)
        {
            _context = context;
        }

        // GET: Positions
        public async Task<IActionResult> Index(string? f)
        {
            ViewBag.F = f;
            return View(await _context.Positions.ToListAsync());
        }

        // GET: Positions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .FirstOrDefaultAsync(m => m.PositionId == id);
            if (position == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "EmployeesPositions", new { id = position.PositionId, nema = position.Position1 });
        }

        // GET: Positions/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create(string? f)
        {
            ViewBag.F = f;
            return View();
        }

        // POST: Positions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PositionId,Position1")] Position position)
        {
            if (ModelState.IsValid && IsUnique(position.Position1))
            {
                _context.Add(position);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Create", "Positions", new { f = "Така позиція вже існує" });
        }

        bool IsUnique(string position)
        {
            //var q = (from pos in _context.Positions
            //         where pos.Position1 == position
            //         select pos).ToList();
            var q1 = _context.Positions.Where(p => p.Position1 == position).ToList();
            if (q1.Count == 0) { return true; }
            return false;
        }

        // GET: Positions/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id, string? f)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.F = f;
            var position = await _context.Positions.FindAsync(id);
            if (position == null)
            {
                return NotFound();
            }
            return View(position);
        }

        // POST: Positions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PositionId,Position1")] Position position)
        {
            if (id != position.PositionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid&&IsUnique(position.Position1))
            {
                try
                {
                    _context.Update(position);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositionExists(position.PositionId))
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
            return RedirectToAction("Edit", "Positions", new { f = "Така позиція вже існує" });
        }

        // GET: Positions/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .FirstOrDefaultAsync(m => m.PositionId == id);
            if (position == null)
            {
                return NotFound();
            }
            var employeePosition = await _context.EmployeesPositions.FirstOrDefaultAsync(m => m.PositionId == id);
            if (employeePosition != null)
            {
                return RedirectToAction("Index", "Positions", new { f = "В цій позиції ще є працівники" });
            }
            return View(position);
        }

        // POST: Positions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PositionExists(int id)
        {
            return _context.Positions.Any(e => e.PositionId == id);
        }
    }
}
