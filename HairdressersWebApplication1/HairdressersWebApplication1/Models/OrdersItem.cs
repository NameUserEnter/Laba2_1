using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class OrdersItem
    {
        public int OrdersItemId { get; set; }

        public int OrderId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Сервіс")]
        public int ServiceId { get; set; }

        public virtual Order Order { get; set; } = null!;
        [Display(Name = "Сервіс")]
        public virtual Service Service { get; set; } = null!;
    }
}
