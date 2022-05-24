using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public partial class Gender
    {
        public Gender()
        {
            Clients = new HashSet<Client>();
            Employees = new HashSet<Employee>();
            Services = new HashSet<Service>();
        }

        public int GenderId { get; set; }
        [Display(Name = "Гендер")]
        public string Gender1 { get; set; } = null!;

        public virtual ICollection<Client> Clients { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}
