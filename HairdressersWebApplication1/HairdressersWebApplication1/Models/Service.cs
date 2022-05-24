using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class Service
    {
        public Service()
        {
            OrdersItems = new HashSet<OrdersItem>();
        }

        public int ServiceId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Назва")]
        public string Title { get; set; } = null!;
        [Display(Name = "Гендер")]
        public int GenderId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Ціна")]
        [Range(1, 2000)]
        public int Price { get; set; }
        [Display(Name = "Гендер")]
        public virtual Gender Gender { get; set; } = null!;
        public virtual ICollection<OrdersItem> OrdersItems { get; set; }
    }
}
