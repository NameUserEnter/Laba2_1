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
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HairdressersWebApplication1.Controllers
{
    [Authorize(Roles = "admin, user, worker")]
    public class OrdersController : Controller
    {
        private readonly HairdressersContext _context;
        private readonly UserManager<User> _userManager;

        public OrdersController(HairdressersContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index(int? id, string ClientEmail)
        {
            if (id == null)
            {
                if (User.IsInRole("user"))
                {
                    var clientId = await _context.Clients.Where(cl => cl.Email == ClientEmail).Select(cl => cl.ClientId).FirstOrDefaultAsync();
                    var orders = _context.Orders.Where(b => b.ClientId == clientId).Include(b => b.Branch).Include(b => b.Client).Include(b => b.Employee);
                    ViewBag.ClientIdd = clientId;
                    List<int> ids = new List<int>();
                    Dictionary<int, int> people = new Dictionary<int, int>();
                    foreach (var o in orders)
                        people.Add(o.OrderId, Price(o.OrderId));
                    ViewBag.OrderPrice = people;
                    return View(await orders.ToListAsync());
                }
                else if (User.IsInRole("worker"))
                {
                    var employeeId = await _context.Employees.Where(cl => cl.Email == ClientEmail).Select(cl => cl.EmployeeId).FirstOrDefaultAsync();
                    var orders = _context.Orders.Where(b => b.EmployeeId == employeeId).Include(b => b.Branch).Include(b => b.Client).Include(b => b.Employee);
                    List<int> ids = new List<int>();
                    Dictionary<int, int> people = new Dictionary<int, int>();
                    foreach (var o in orders)
                        people.Add(o.OrderId, Price(o.OrderId));
                    ViewBag.OrderPrice = people;
                    return View(await orders.ToListAsync());
                }
                if (User.IsInRole("admin"))
                {
                    var orders = _context.Orders.Include(b => b.Branch).Include(b => b.Client).Include(b => b.Employee);
                    List<int> ids = new List<int>();
                    Dictionary<int, int> people = new Dictionary<int, int>();
                    foreach (var o in orders)
                        people.Add(o.OrderId, Price(o.OrderId));
                    ViewBag.OrderPrice = people;
                    return View(await orders.ToListAsync());
                }

            }
            //return RedirectToAction("Index", "OrdersItems");
            ViewBag.OrderPrice = Price(id);
            var order = _context.Orders.Where(b => b.OrderId == id).Include(b => b.Branch).Include(b => b.Client).Include(b => b.Employee);
            return View(await order.ToListAsync());
        }
        //OrderPrice
        public int Price(int? id)
        {
            var ordersItemsServices = _context.OrdersItems.Where(b => b.OrderId == id).Select(b => b.Service).ToList();
            int sum = 0;
            foreach (var services in ordersItemsServices)
                sum += services.Price;
            return sum;
        }
        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.OrderIdd = id;
            var order = await _context.Orders
                .Include(o => o.Branch)
                .Include(o => o.Client)
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            //return View(order);
            return RedirectToAction("Index", "OrdersItems", new { id = id });
        }

        // GET: Orders/Create
        public IActionResult Create(int? id, string ClientEmail, string? Time)
        {
            ViewBag.ClientIdd = _context.Clients.Where(cl => cl.Email == ClientEmail).Select(cl => cl.ClientId).FirstOrDefault();
            if (id != null)
                ViewBag.ClientIdd = id;
            ViewBag.Time = Time;
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address");
            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "LastName");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName");
            ViewData["ServiceId"] = _context.Services.ToList();
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderDate,ClientId,EmployeeId,BranchId")] Order order, List<string> services)
        {
            //await _userManager.GetUserNameAsync(User);
            //string value = User.FindAll("preferred_username").First().Value;
            //User applicationUser = await _userManager.GetUserAsync(User);
            //string userEmail = User?.Identity.Name;
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //User use = await _userManager.FindByIdAsync(userId);
            // order.ClientId = await _context.Clients.Where(cl => cl.Email == ClientEmail).Select(cl=>cl.ClientId).FirstOrDefaultAsync();
            //var clientId = _context.Clients.Where(cl => cl.Email == ClientEmail).Select(cl => cl.ClientId).FirstOrDefault();
            //order.ClientId = clientId;
            if (services.Count() == 0) {
                ViewBag.Time = "Оберіть хоча б один сервіс";
                return RedirectToAction("Create", "Orders", new { Time = ViewBag.Time, ClientEmail = _context.Clients.Where(cl => cl.ClientId == order.ClientId).Select(cl => cl.Email).FirstOrDefault() }); }
            var time = Time(order.OrderDate, order.EmployeeId, order.ClientId);
            if (time)
            {
                var res = Next_time(order.OrderDate, order.EmployeeId);
                ViewBag.Time = $"Не корректно введений формат дати або у цей час вже є замовлення. Залишається вільний час о {res}";
                ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", order.BranchId);
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", order.EmployeeId);
                return RedirectToAction("Create", "Orders", new { Time = ViewBag.Time, ClientEmail = _context.Clients.Where(cl => cl.ClientId == order.ClientId).Select(cl => cl.Email).FirstOrDefault() });

            }
            else if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                if (services.Count > 0)
                {
                    foreach (var serv in services)
                    {
                        var serviceId = _context.Services.Where(s => s.Title == serv).Select(s => s.ServiceId).FirstOrDefault();
                        OrdersItem o = new OrdersItem { OrderId = order.OrderId, ServiceId = serviceId };
                        _context.Add(o);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction("Index", "Orders", new { ClientEmail = _context.Clients.Where(cl => cl.ClientId == order.ClientId).Select(cl => cl.Email).FirstOrDefault() });
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", order.BranchId);
            //ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "ClientId", order.ClientId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", order.EmployeeId);
            return RedirectToAction("Index", "Orders", new { ClientEmail = _context.Clients.Where(cl => cl.ClientId == order.ClientId).Select(cl => cl.Email).FirstOrDefault() });

        }

        //Work with time
        public bool Time(DateTime time, int e_id, int c_id)
        {
            bool decision = true;
            if (time > DateTime.Today)
            {
                bool d = false;
                var times_e = _context.Orders.Where(b => b.EmployeeId == e_id).Select(b => b.OrderDate).ToList();
                var times_c = _context.Orders.Where(b => b.ClientId == c_id).Select(b => b.OrderDate).ToList();
                if (times_e.Count()==0|| times_c.Count() == 0)
                    return false;
                times_e.Sort();
                times_c.Sort();
                if (time == times_c[times_c.Count() - 1].AddHours(1) || time == times_e[times_e.Count() - 1].AddHours(1))
                    return false;
                for (int t = 0; t <= times_e.Count()-1; t++)
                    if (time == times_e[t])
                        return decision;
                for (int t = 0; t <= times_c.Count()-1; t++)
                    if (time == times_c[t])
                        return decision;
                if (time.Minute == 0 && time.Second == 0 && time.Millisecond == 0&&time.Hour>=7 && time.Hour <= 21)
                    d = false;
                else return decision;
                decision = false;
            }
            return decision;
        }
        public string Next_time(DateTime time, int e_id)
        {
            var ordH = _context.Orders.Where(b => b.EmployeeId == e_id && b.OrderDate.Day == time.Day).Select(b => b.OrderDate.Hour).ToList();
            List<int> timess = new List<int>();
            for (int t = 7; t <= 21; t++)
                timess.Add(t);
            foreach (var t in ordH)
                timess.Remove(t);
            List<string> strings = timess.ConvertAll<string>(x => x.ToString());
            var resullt = String.Join(".00, ", strings.ToArray());
            resullt += ".00";
            return resullt;
        }
        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string? Time, int? id, int? clId)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.OrderIdd = id;
            ViewBag.ClientIdd = clId;
            ViewBag.Time = Time;
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", order.BranchId);
            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "LastName", order.ClientId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", order.EmployeeId);
            ViewData["ServiceId"] = _context.Services.ToList();
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderDate,ClientId,EmployeeId,BranchId")] Order order, List<string> services)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }
            if (User.IsInRole("user")&&services.Count() == 0)
            {
                ViewBag.Time = "Оберіть хоча б один сервіс";
                return RedirectToAction("Edit", "Orders", new { Time = ViewBag.Time, id = order.OrderId, clId = order.ClientId });
            }
            if (User.IsInRole("admin"))
            {
                var servisss = _context.OrdersItems.Where(o => o.OrderId == id).Select(o => o.ServiceId).ToList();
            }
            var time = Time(order.OrderDate, order.EmployeeId, order.ClientId);
            if (order.OrderDate != _context.Orders.Where(o => o.OrderId == id).Select(o => o.OrderDate).FirstOrDefault())
            {
                if (time)
                {
                    var res = Next_time(order.OrderDate, order.EmployeeId);
                    ViewBag.Time = $"Не корректно введений формат дати або у цей час вже є замовлення. Залишається вільний час о {res}";
                    ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", order.BranchId);
                    ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", order.EmployeeId);
                    return RedirectToAction("Edit", "Orders", new { Time = ViewBag.Time, id = order.OrderId, clId = order.ClientId });
                }
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    if (User.IsInRole("user") && services.Count > 0)
                    {
                        var sss = _context.OrdersItems.Where(s => s.OrderId == id).ToList();
                        foreach (var ss in sss)
                        {
                            _context.OrdersItems.Remove(ss);
                            await _context.SaveChangesAsync();
                        }
                        foreach (var serv in services)
                        {
                            var serviceId = _context.Services.Where(s => s.Title == serv).Select(s => s.ServiceId).FirstOrDefault();
                            OrdersItem o = new OrdersItem { OrderId = order.OrderId, ServiceId = serviceId };
                            _context.Add(o);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", order.BranchId);
            //ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "ClientId", order.ClientId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "LastName", order.EmployeeId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.OrderIdd = id;
            var order = await _context.Orders
                .Include(o => o.Branch)
                .Include(o => o.Client)
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            var orderItems = await _context.OrdersItems.Where(o => o.OrderId == id).ToListAsync();
            foreach (var item in orderItems)
                _context.OrdersItems.Remove(item);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Orders", new { ClientEmail = _context.Clients.Where(cl => cl.ClientId == order.ClientId).Select(cl => cl.Email).FirstOrDefault() });
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
