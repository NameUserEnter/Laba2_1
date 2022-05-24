using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class EmployeesPosition
    {
        public int EmployeesPositionId { get; set; }
        [Display(Name = "Позиція")]
        public int PositionId { get; set; }
        [Display(Name = "Працівник")]
        public int EmployeeId { get; set; }
        [Display(Name = "Працівник")]

        public virtual Employee Employee { get; set; } = null!;
        [Display(Name = "Позиція")]
        public virtual Position Position { get; set; } = null!;
    }
}
