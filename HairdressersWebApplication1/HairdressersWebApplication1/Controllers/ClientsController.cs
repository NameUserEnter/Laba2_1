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
using Microsoft.AspNetCore.Identity;

namespace HairdressersWebApplication1.Controllers
{
    [Authorize(Roles = "admin, user")]
    public class ClientsController : Controller
    {
        private readonly HairdressersContext _context;
        private readonly UserManager<User> _userManager;
        public ClientsController(HairdressersContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Clients
        public async Task<IActionResult> Index(int? id)
        {
            var client = _context.Clients.Include(e => e.Gender).ToListAsync();
            return View(await client);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Gender)
                .FirstOrDefaultAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create(int? id)
        {
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1");
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,FirstName,LastName,Phone,GenderId,Note")] Client client)
        {
            if (ModelState.IsValid && IsUnique(client.Phone))
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1", client.GenderId);
            return View(client);
        }

        bool IsUnique(string phone)
        {
            var q = (from cl in _context.Clients
                     where cl.Phone == phone
                     select cl).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderId", client.GenderId);
            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,FirstName,LastName,Phone,GenderId,Note")] Client client)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.ClientId))
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
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderId", client.GenderId);
            return View(client);
        }

        // GET: Clients/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Gender)
                .FirstOrDefaultAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.Orders.Where(m => m.ClientId == id).ToListAsync();
            if (orders != null)
                foreach (var order in orders)
                {
                    var orderItems = await _context.OrdersItems.Where(o => o.OrderId == order.OrderId).ToListAsync();
                    foreach (var item in orderItems)
                    {
                        _context.OrdersItems.Remove(item);
                        await _context.SaveChangesAsync();
                    }
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
            var client = await _context.Clients.FindAsync(id);
            var idcc = client.Email;
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            if (_context.Employees.Where(e => e.Email == idcc).ToList().Count()==0)
            {
                User user = await _userManager.FindByEmailAsync(idcc);
                if (user != null)
                {
                    IdentityResult result = await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}
