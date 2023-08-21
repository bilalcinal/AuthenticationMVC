using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AuthenticationController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register(int Id)
        {
            ViewData["Id"] = Id;
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register(AccountModel accountModel)
        {
             var existingUser = await _applicationDbContext.Accounts
            .Where(a => a.Email == accountModel.Email || a.Phone == accountModel.Phone).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                if (existingUser.Email == accountModel.Email && existingUser.Phone == accountModel.Phone)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi ve telefon numarası zaten kullanılıyor.");
                }
                else if (existingUser.Email == accountModel.Email)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                }
                else if (existingUser.Phone == accountModel.Phone)
                {
                    ModelState.AddModelError("Phone", "Bu telefon numarası zaten kullanılıyor.");
                }

                return View(accountModel);
            }

            if (ModelState.IsValid)
            {
                _applicationDbContext.Accounts.Add(new Account
                {
                    FirstName = accountModel.FirstName,
                    LastName = accountModel.LastName,
                    Email = accountModel.Email,
                    Phone = accountModel.Phone,
                    Password = accountModel.Password
                });

                await _applicationDbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                return RedirectToAction("Login", "Authentication");
            }

            return View(accountModel);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var user = await _applicationDbContext.Accounts.FirstOrDefaultAsync(a => a.Email == loginModel.Email && a.Password == loginModel.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Geçersiz e-posta adresi veya şifre.");
                return View(loginModel);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email)
                
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("UserInfo", "Authentication"); 
        }
        public async Task<IActionResult> UserInfo()
        {
            var userEmail = User.Identity.Name;
            
            var user = await _applicationDbContext.Accounts.Where(a => a.Email == userEmail).FirstOrDefaultAsync();

            if (user != null)
            {
                var userInfoModel = new UserInfoModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone
                };

                return View(userInfoModel);
            }

            return RedirectToAction("Index", "Home");
        }

    }
}

