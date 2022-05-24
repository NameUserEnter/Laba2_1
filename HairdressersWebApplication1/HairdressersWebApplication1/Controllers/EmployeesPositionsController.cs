#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HairdressersWebApplication1;

namespace HairdressersWebApplication1.Controllers
{
    [Authorize(Roles = "admin, user, worker")]
    public class EmployeesPositionsController : Controller
    {
        private readonly HairdressersContext _context;

        public EmployeesPositionsController(HairdressersContext context)
        {
            _context = context;
        }

        // GET: EmployeesPositions
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null)
                return RedirectToAction("Index", "Services");
            ViewBag.PositionIdd = id;
            ViewBag.PositionIName = name;
            var emPosition = _context.EmployeesPositions.Where(b => b.PositionId == id).Include(e => e.Employee).Include(e => e.Position);
            return View(await emPosition.ToListAsync());
        }

        // GET: EmployeesPositions/Details/5
        public async Task<IActionResult> Details(int? id, int positionId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeesPosition = await _context.EmployeesPositions
                .Include(e => e.Employee)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.EmployeesPositionId == id);
            if (employeesPosition == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Employees", new { id = employeesPosition.EmployeeId });
        }

        // GET: EmployeesPositions/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create(int positionId, string? f)
        {
            ViewBag.F = f;
            ViewBag.PositionIdd = positionId;
            ViewBag.PositionName = _context.Positions.Where(i => i.PositionId == positionId).Select(i => i.Position1).FirstOrDefault();
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName");
            return View();
        }

        // POST: EmployeesPositions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int positionId,[Bind("EmployeesPositionId,PositionId,EmployeeId")] EmployeesPosition employeesPosition)
        {
            employeesPosition.PositionId =positionId;
            if(IsUnique(employeesPosition.EmployeeId, employeesPosition.PositionId) == false)
                return RedirectToAction("Create", "EmployeesPositions", new { f = "Користувач з такою позицією вже існує" , id = positionId, name = _context.Positions.Where(i => i.PositionId == positionId).FirstOrDefault().Position1 });
            if (ModelState.IsValid)
            {
                _context.Add(employeesPosition);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "EmployeesPositions", new { id = positionId, name = _context.Positions.Where(i => i.PositionId == positionId).FirstOrDefault().Position1 });
            }

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", employeesPosition.EmployeeId);
            //ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "Position1", employeesPosition.PositionId);
            return RedirectToAction("Index", "EmployeesPositions", new { id = positionId, name = _context.Positions.Where(i => i.PositionId == positionId).FirstOrDefault().Position1 });
        }
        bool IsUnique(int empId, int posId)
        {
            var q1 = _context.EmployeesPositions.Where(e => e.EmployeeId == empId && e.PositionId == posId).ToList();
            if (q1.Count() == 0) { return true; }
            return false;
        }
        // GET: EmployeesPositions/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id, int positionId)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.PositionIdd = positionId;
            var employeesPosition = await _context.EmployeesPositions.FindAsync(id);
            if (employeesPosition == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", employeesPosition.EmployeeId);
            //ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionId", employeesPosition.PositionId);
            return View(employeesPosition);
        }

        // POST: EmployeesPositions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,int positionId, [Bind("EmployeesPositionId,PositionId,EmployeeId")] EmployeesPosition employeesPosition)
        {
            if (id != employeesPosition.EmployeesPositionId)
            {
                return NotFound();
            }
            employeesPosition.PositionId = positionId;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeesPosition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeesPositionExists(employeesPosition.EmployeesPositionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "EmployeesPositions", new { id = positionId, name = _context.Positions.Where(i => i.PositionId == positionId).FirstOrDefault().Position1 });
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", employeesPosition.EmployeeId);
            //ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionId", employeesPosition.PositionId);
            return View(employeesPosition);
        }

        // GET: EmployeesPositions/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id, int positionId)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.PositionIdd = positionId;
            var employeesPosition = await _context.EmployeesPositions
                .Include(e => e.Employee)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.EmployeesPositionId == id);
            if (employeesPosition == null)
            {
                return NotFound();
            }

            return View(employeesPosition);
        }

        // POST: EmployeesPositions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int positionId)
        {
            var employeesPosition = await _context.EmployeesPositions.FindAsync(id);
            _context.EmployeesPositions.Remove(employeesPosition);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "EmployeesPositions", new { id = positionId, name = _context.Positions.Where(i => i.PositionId == positionId).FirstOrDefault().Position1 });
        }

        private bool EmployeesPositionExists(int id)
        {
            return _context.EmployeesPositions.Any(e => e.EmployeesPositionId == id);
        }
    }
}
