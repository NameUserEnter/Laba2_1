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
    public class OrdersItemsController : Controller
    {
        private readonly HairdressersContext _context;

        public OrdersItemsController(HairdressersContext context)
        {
            _context = context;
        }

        // GET: OrdersItems
        public async Task<IActionResult> Index(int? id, int? s_id)
        {
            if (s_id != null)
            {
                ViewBag.ServicesIdd = s_id;
                var OrdersItems = _context.OrdersItems.Where(b => b.ServiceId == s_id).Include(b => b.Order).Include(b => b.Service);
                return View(await OrdersItems.ToListAsync());
            }
            if (id == null)
                return RedirectToAction("Index","Services");
            ViewBag.OrderIdd = id;
            //ViewBag.ServiceName = name;
            var ordersItems = _context.OrdersItems.Where(b => b.OrderId == id).Include(b => b.Order).Include(b => b.Service);
            //var hairdressersContext = _context.OrdersItems.Include(o => o.Order).Include(o => o.Service);
            return View(await ordersItems.ToListAsync());
        }

        // GET: OrdersItems/Details/5
        public async Task<IActionResult> Details(int? id, int serviceId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordersItem = await _context.OrdersItems
                .Include(o => o.Order)
                .Include(o => o.Service)
                .FirstOrDefaultAsync(m => m.OrdersItemId == id);
            if (ordersItem == null)
            {
                return NotFound();
            }
            ViewBag.ServiceIdd = serviceId;
            return View(ordersItem);
            //return RedirectToAction("Index", "Orders", new { id = ordersItem.OrderId });
        }

        // GET: OrdersItems/Create
        public IActionResult Create(int OrderIdd)
        {
            ViewBag.OrderIdd = OrderIdd;
            //ViewBag.ServiceIdd = serviceId;
            //ViewBag.ServiceName = _context.Services.Where(i => i.ServiceId == serviceId).FirstOrDefault().Title;
            //ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId");
            //var ordersItemsss = _context.OrdersItems.Where(b => b.ServiceId == serviceId).Include(b => b.ServiceId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Title");
            return View();
        }

        // POST: OrdersItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrdersItemId,OrderId,ServiceId")] OrdersItem ordersItem, int orderId)
        {
            //ordersItem.ServiceId = serviceId;
            ordersItem.OrderId = orderId;
            if (ModelState.IsValid && IsUnique(orderId, ordersItem.ServiceId))
            {
                _context.OrdersItems.Add(ordersItem);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index","OrdersItems", new { id = ordersItem.OrderId });
            };
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Title", ordersItem.ServiceId);
            //ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", ordersItem.OrderId);
            //var ordersItemsss = _context.OrdersItems.Where(b => b.ServiceId == serviceId).Include(b => b.ServiceId);
            //ViewData["ServiceId"] = new SelectList(_context.Orders, "ServiceId", "ServiceId", ordersItem.ServiceId);
            //return View(ordersItem);
            return RedirectToAction("Index","OrdersItems", new { id = ordersItem.OrderId });
        }
        bool IsUnique(int orderId, int serviceId)
        {
            var q = (from or in _context.OrdersItems
                     where or.OrderId == orderId 
                     select or.ServiceId).ToList();
            foreach(var ser in q)
            if (ser == serviceId) { return false; }
            return true;
        }
        // GET: OrdersItems/Edit/5
        public async Task<IActionResult> Edit(int? id, int serviceId)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.ServiceIdd = serviceId;
            var ordersItem = await _context.OrdersItems.FindAsync(id);
            if (ordersItem == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", ordersItem.OrderId);
            //ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Title", ordersItem.ServiceId);
            return View(ordersItem);
        }

        // POST: OrdersItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int serviceId, [Bind("OrdersItemId,OrderId,ServiceId")] OrdersItem ordersItem)
        {
            if (id != ordersItem.OrdersItemId)
            {
                return NotFound();
            }
            ordersItem.ServiceId = serviceId;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordersItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersItemExists(ordersItem.OrdersItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "OrdersItems", new { id = serviceId, name = _context.Services.Where(i => i.ServiceId == serviceId).FirstOrDefault().Title });
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", ordersItem.OrderId);
            //ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Title", ordersItem.ServiceId);
            return View(ordersItem);
        }

        // GET: OrdersItems/Delete/5
        public async Task<IActionResult> Delete(int? id,int ? orderId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordersItem = await _context.OrdersItems
                .Include(o => o.Order)
                .Include(o => o.Service)
                .FirstOrDefaultAsync(m => m.OrdersItemId == id);
            if (ordersItem == null)
            {
                return NotFound();
            }
            ViewBag.OrderIdd = orderId;
            return View(ordersItem);
        }

        // POST: OrdersItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int orderId)
        {
            var ordersItem = await _context.OrdersItems.FindAsync(id);
            _context.OrdersItems.Remove(ordersItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "OrdersItems", new { id = orderId});
           // return RedirectToAction(nameof(Index));
        }

        private bool OrdersItemExists(int id)
        {
            return _context.OrdersItems.Any(e => e.OrdersItemId == id);
        }
    }
}
