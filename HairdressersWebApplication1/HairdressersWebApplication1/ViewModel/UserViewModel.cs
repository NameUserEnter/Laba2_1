using System.ComponentModel.DataAnnotations;

namespace HairdressersWebApplication1.ViewModel
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Range(1945, 2017)]
        [Display(Name = "Рік народжуння")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Підтвердження пароля")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
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
        [Required]
        [Display(Name = "Гендер")]
        public int GenderId { get; set; }
        [Display(Name = "Замітки")]
        [StringLength(500)]
        public string? Note { get; set; }
        [Display(Name = "Гендер")]
        public virtual Gender Gender { get; set; } = null!;
    }
    public class EditUserViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Range(1945, 2017)]
        [Display(Name = "Рік народжуння")]
        public int Year { get; set; }
    }
    public class ChangePasswordViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути пустим")]
        public string NewPassword { get; set; }
    }
}
