using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class Branch
    {
        public Branch()
        {
            Orders = new HashSet<Order>();
        }

        public int BranchId { get; set; }
        [Required(ErrorMessage ="Поле не повинно бути пустим")]
        [Display(Name ="Адреса")]
        public string Address { get; set; } = null!;
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Назва")]
        public string Title { get; set; } = null!;
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [RegularExpression(@"[0-9]{10}", ErrorMessage = "Некоректний телефон")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }
    }
}
