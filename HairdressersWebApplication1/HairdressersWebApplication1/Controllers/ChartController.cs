using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace HairdressersWebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,user,worker")]
    public class ChartController : ControllerBase
    {
        private readonly HairdressersContext _context;

        public ChartController(HairdressersContext context)
        {
            _context = context;
        }

        [HttpGet("JsonData")]
        public JsonResult JsonData()
        {
            var services = _context.Services.ToList();
            List<object> catOrder = new List<object>();
            catOrder.Add(new[] {"Зачіска","Кількість замовлення"});
            foreach (var s in services)
                catOrder.Add(new object[] { s.Title, _context.OrdersItems.Where(b => b.ServiceId == s.ServiceId).Count() });
            return new JsonResult(catOrder);
        }

        [HttpGet("JsonD")]
        public JsonResult JsonD()
        {
            var employees = _context.Employees.ToList();
            List<object> catOrder = new List<object>();
            catOrder.Add(new[] { "Перукар", "Кількість замовлення" });
            foreach (var e in employees)
            {
                var orders = _context.Orders.Where(o => o.EmployeeId == e.EmployeeId).ToList();
                int count_o = 0;
                foreach (var order in orders)
                {
                   count_o +=  _context.OrdersItems.Where(b => b.OrderId == order.OrderId).Count() ;
                }
                catOrder.Add(new object[] { e.LastName,count_o });
            }
            return new JsonResult(catOrder);
        }

    }
}
