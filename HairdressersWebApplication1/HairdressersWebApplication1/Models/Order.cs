using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class Order
    {
        public Order()
        {
            OrdersItems = new HashSet<OrdersItem>();
        }

        public int OrderId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Час")]
        public DateTime OrderDate { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Клієнт")]
        public int ClientId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Працівник")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Відділення")]
        public int BranchId { get; set; }
        [Display(Name = "Відділення")]
        public virtual Branch Branch { get; set; } = null!;
        [Display(Name = "Клієнт")]
        public virtual Client Client { get; set; } = null!;
        [Display(Name = "Працівник")]
        public virtual Employee Employee { get; set; } = null!;
        public virtual ICollection<OrdersItem> OrdersItems { get; set; }
    }
}
