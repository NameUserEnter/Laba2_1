using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class Employee
    {
        public Employee()
        {
            EmployeesPositions = new HashSet<EmployeesPosition>();
            Orders = new HashSet<Order>();
        }

        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Ім'я")]
        public string FirstName { get; set; } = null!;
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Прізвище")]
        public string LastName { get; set; } = null!;
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [RegularExpression(@"[0-9]{10}", ErrorMessage = "Некоректний телефон")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = null!;
        //[Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Display(Name = "Гендер")]
        public int GenderId { get; set; }
        [Display(Name = "Замітки")]
        [StringLength(500)]
        public string? Note { get; set; }
        [Display(Name = "Гендер")]

        public virtual Gender Gender { get; set; } = null!;
        public virtual ICollection<EmployeesPosition> EmployeesPositions { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
