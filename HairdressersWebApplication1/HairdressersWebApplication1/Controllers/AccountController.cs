using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HairdressersWebApplication1.ViewModel;
using HairdressersWebApplication1;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HairdressersWebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly HairdressersContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, HairdressersContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "Gender1");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid && IsUnique(model.Phone))
            {
                User user = new User { Email = model.Email, UserName = model.Email, Year = model.Year };
                Client client = new Client { FirstName = model.FirstName, LastName = model.LastName, Phone = model.Phone, 
                                Email = model.Email, GenderId = model.GenderId, Note = model.Note};
                // додаємо користувача
                    var result = await _userManager.CreateAsync(user, model.Password);
                    _context.Add(client);
                    await _context.SaveChangesAsync();
                if (result.Succeeded)
                {
                    // установка кукі
                    await _signInManager.SignInAsync(user, false);
                    // генерация токена для пользователя
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, code = code },
                        protocol: HttpContext.Request.Scheme);
                    EmailService emailService = new EmailService();
                    await emailService.SendEmailAsync(model.Email, "Confirm your account",
                        $"Подтвердите регистрацию, перейдя по ссылке: <a href='{callbackUrl}'>link</a>");

                    return Content("Для завершения регистрации проверьте электронную почту и перейдите по ссылке, указанной в письме");
                    //return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        bool IsUnique(string phone)
        {
            var q = (from cl in _context.Clients
                     where cl.Phone == phone
                     select cl).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            if (await _userManager.IsInRoleAsync(user, "admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null && _context.Employees.Where(e=>e.Email==model.Email).ToList()==null)
                {
                    // проверяем, подтвержден ли email
                    if ((!await _userManager.IsEmailConfirmedAsync(user)) && (!await _userManager.IsInRoleAsync(user, "admin")) && (!await _userManager.IsInRoleAsync(user, "worker")))
                    {
                        ModelState.AddModelError(string.Empty, "Вы не подтвердили свой email");
                        return View(model);
                    }
                }
                    var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // перевіряємо, чи належить URL додатку
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        if (await _userManager.IsInRoleAsync(user, "worker") && (!_context.Employees.ToList().Contains(_context.Employees.Where(e => e.Email == model.Email).FirstOrDefault())))
                        {
                            var client = _context.Clients.Where(c => c.Email == model.Email).FirstOrDefault();
                            Employee employee = new Employee
                            {
                                FirstName = client.FirstName,
                                LastName = client.LastName,
                                Phone = client.Phone,
                                Email = client.Email,
                                GenderId = client.GenderId,
                                Note = client.Note
                            };
                            _context.Add(employee);
                            await _context.SaveChangesAsync();
                        }
                        if(!(await _userManager.IsInRoleAsync(user, "worker")||
                            await _userManager.IsInRoleAsync(user, "user") ||
                            await _userManager.IsInRoleAsync(user, "admin")))
                        {
                            var userRoles = await _userManager.GetRolesAsync(user);
                            userRoles.Add("user");
                            await _userManager.AddToRolesAsync(user, userRoles);
                            await _signInManager.SignOutAsync();
                            await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильний логін чи (та) пароль");
                }
            }
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // видаляємо аутентифікаційні куки
            await _signInManager.SignOutAsync();
            //if (Request.Cookies["Identity.External"] != null)
            //{
            //    Response.Cookies.Delete("Identity.External");
            //}
            return RedirectToAction("Index", "Home");
        }

    }
}
