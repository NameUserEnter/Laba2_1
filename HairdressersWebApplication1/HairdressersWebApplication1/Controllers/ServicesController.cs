#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using HairdressersWebApplication1;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace HairdressersWebApplication1.Controllers
{
    [Authorize(Roles = "admin, user, worker")]
    public class ServicesController : Controller
    {
        private readonly HairdressersContext _context;
        private readonly UserManager<User> _userManager;
        public ServicesController(HairdressersContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Services
        public async Task<IActionResult> Index(string? f,string? f1)
        {
            ViewBag.F1 = f1;
            ViewBag.F = f;
            var hairdressersContext = _context.Services.Include(s => s.Gender);
            return View(await hairdressersContext.ToListAsync());
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Gender)
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "OrdersItems", new { s_id = service.ServiceId});
        }

        // GET: Services/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create(string? f)
        {
            ViewBag.F = f;
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1");
            return View();
        }

        // POST: Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceId,Title,GenderId,Price")] Service service)
        {
            if (ModelState.IsValid && IsUnique(service.Title))
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1", service.GenderId);
            return RedirectToAction("Create", "Services", new { f = "Такий сервіс вже існує" });
            //return View(service);
        }

        bool IsUnique(string title)
        {
            var q = (from serv in _context.Services
                     where serv.Title == title
                     select serv).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        // GET: Services/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id, string? f)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.F = f;
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1", service.GenderId);
            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,Title,GenderId,Price")] Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid && IsUnique(service.Title))
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceId))
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
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1", service.GenderId);
            return RedirectToAction("Edit", "Services", new { f = "Такий сервіс вже існує" });
        }

        // GET: Services/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Gender)
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }
            var ordersItems = await _context.OrdersItems.FirstOrDefaultAsync(m => m.ServiceId == id);
            if (ordersItems != null)
            {
                return RedirectToAction("Index", "Services", new { f1 = "В цьому сервісі ще є замовлення" });
            }
            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel)
        {
            if (ModelState.IsValid)
            {
                if (fileExcel != null)
                {
                    var stream = new FileStream(fileExcel.FileName, FileMode.Create);
                    await fileExcel.CopyToAsync(stream);
                    try
                    {
                        XLWorkbook workBook = new XLWorkbook(stream, XLEventTracking.Disabled);
                        foreach (IXLWorksheet worksheet in workBook.Worksheets)
                        {
                            Gender newcat;
                            var c = (from cat in _context.Genders where cat.Gender1.Contains(worksheet.Name) select cat).ToList();
                            if (c.Count > 0)
                            {
                                newcat = c[0];
                            }
                            else
                            {
                                newcat = new Gender();
                                newcat.Gender1 = worksheet.Name;
                                _context.Genders.Add(newcat);
                            }
                            var ex = AllRows(worksheet, newcat);
                            if (ex != null)
                            {
                                workBook.Dispose();
                                stream.Dispose();
                                return RedirectToAction("Index", "Services", new { f = "Не коректні дані" });
                            }
                        }
                        workBook.Dispose();
                        stream.Dispose();
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Index", "Services", new { f = "Не корректний формат файлу" });
                    }
                }
                else
                    return RedirectToAction("Index", "Services", new { f = "Не прикріплений файл" });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public string? AllRows(IXLWorksheet worksheet, Gender newcat)
        {
                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                {
                    try
                    {
                        Service serv = new Service();
                        serv.Title = row.Cell(1).Value.ToString();
                        serv.Price = Convert.ToInt32(row.Cell(2).Value);
                        serv.Gender = newcat;
                        if (_context.Services.Where(s => s.Title == serv.Title && s.Price != serv.Price).ToList().Count() > 0)
                        { return "Не корректні дані";}
                        if(_context.Services.Where(s => s.Title == serv.Title).ToList().Count()==0)
                        _context.Services.Add(serv);
                        for (int i = 3; i <= 7; i++)
                        {
                            if (row.Cell(i).Value.ToString().Length > 0)
                            {
                                Position pos;

                                var a = (from aut in _context.Positions
                                         where aut.Position1.Contains(row.Cell(i).Value.ToString())
                                         select aut).ToList();
                                if (a.Count > 0)
                                {
                                    pos = a[0];
                                }
                                else
                                {
                                    pos = new Position();
                                    pos.Position1 = row.Cell(i).Value.ToString();
                                    _context.Positions.Add(pos);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return "Не корректні дані";
                    }
                }
            return null;
        }
        public ActionResult Export()
        {
            using (XLWorkbook workbook = Exporting())
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Flush();

                return new FileContentResult(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"hairdresser_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                };
            }
        }
        public XLWorkbook Exporting()
        {
            XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled);
            var orders = _context.Orders.ToList();
            foreach (var c in orders)
            {
                var worksheet = workbook.Worksheets.Add(c.OrderId);

                worksheet.Cell("A1").Value = "Назва";
                worksheet.Cell("B1").Value = "Час";
                worksheet.Cell("C1").Value = "Ціна";
                worksheet.Cell("D1").Value = "Гендер";
                worksheet.Cell("E1").Value = "Перукар";
                worksheet.Cell("F1").Value = "Відділення";
                worksheet.Cell("G1").Value = "Загалом";
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Column(2).Width = 18;
                var or = _context.OrdersItems.Where(b => b.OrderId == c.OrderId).Include("Service").ToList();
                for (int i = 0; i < or.Count; i++)
                {
                    var ab = _context.Services.Where(a => a.ServiceId == or[i].ServiceId).FirstOrDefault();
                    worksheet.Cell(i + 2, 1).Value = ab.Title;
                    worksheet.Cell(i + 2, 3).Value = ab.Price;
                    worksheet.Cell(i + 2, 4).Value = _context.Genders.Where(a => a.GenderId == ab.GenderId).FirstOrDefault().Gender1;
                    worksheet.Cell(i + 2, 2).Value = c.OrderDate.ToString();
                    worksheet.Cell(i + 2, 5).Value = _context.Employees.Where(a => a.EmployeeId == c.EmployeeId).FirstOrDefault().LastName;
                    worksheet.Cell(i + 2, 6).Value = _context.Branches.Where(a => a.BranchId == c.BranchId).FirstOrDefault().Title;
                }
            }
            return workbook;
        }
    }
}
