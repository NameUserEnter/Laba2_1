using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HairdressersWebApplication1;

namespace HairdressersWebApplication1.Controllers
{
    [Authorize(Roles = "admin, user, worker")]
    public class EmployeesController : Controller
    {
        private readonly HairdressersContext _context;
        private readonly UserManager<User> _userManager;

        public EmployeesController(HairdressersContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employees
        public async Task<IActionResult> Index(int? id, string? f)
        {
            if (id == null)
                return RedirectToAction("Index", "Services");
            ViewBag.F = f;
            var idd = _context.EmployeesPositions.Where(e => e.PositionId == id).Select(e => e.EmployeeId).ToList();
            ViewBag.EmployeeIdd = idd;
            var employee = _context.Employees.Where(e=>e.EmployeeId == id).Include(e => e.Gender).ToListAsync();
            return View(await employee);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Gender)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create(int? id)
        {
            ViewBag.EmployeeIdd = id;
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,FirstName,LastName,Phone,GenderId,Note")] Employee employee)
        {
            if (ModelState.IsValid && IsUnique(employee.Phone))
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1", employee.GenderId);
            return View(employee);
        }

        bool IsUnique(string phone)
        {
            var q = (from em in _context.Employees
                     where em.Phone == phone
                     select em).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        // GET: Employees/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderId", employee.GenderId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FirstName,LastName,Phone,GenderId,Note")] Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
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
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderId", employee.GenderId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Gender)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }
            var orders = await _context.Orders.FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (orders != null)
            {
                return RedirectToAction("Index", "Employees", new { id = orders.EmployeeId, f = "У працівника ще є замовлення" });
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            var employeePosition = await _context.EmployeesPositions.Where(o => o.EmployeeId == id).ToListAsync();
            var client = await _context.Clients.Where(cl=>cl.Email==employee.Email).FirstOrDefaultAsync();
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            var idee = employee.Email;
            foreach (var item in employeePosition)
                _context.EmployeesPositions.Remove(item);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            User user = await _userManager.FindByEmailAsync(idee);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
