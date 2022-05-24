using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class Position
    {
        public Position()
        {
            EmployeesPositions = new HashSet<EmployeesPosition>();
        }

        public int PositionId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Позиція")]
        public string Position1 { get; set; } = null!;

        public virtual ICollection<EmployeesPosition> EmployeesPositions { get; set; }
    }
}
