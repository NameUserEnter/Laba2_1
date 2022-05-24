﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Range(1945, 2017)]
        [Display(Name = "Рік народжуння")]
        public int Year { get; set; }
    }
}
