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
             var existingAccount = await _applicationDbContext.Accounts
            .Where(a => a.Email == accountModel.Email || a.Phone == accountModel.Phone).FirstOrDefaultAsync();

            if (existingAccount != null)
            {
                if (existingAccount.Email == accountModel.Email && existingAccount.Phone == accountModel.Phone)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi ve telefon numarası zaten kullanılıyor.");
                }
                else if (existingAccount.Email == accountModel.Email)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                }
                else if (existingAccount.Phone == accountModel.Phone)
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
            var account = await _applicationDbContext.Accounts.Where(a => a.Email == loginModel.Email && a.Password == loginModel.Password).FirstOrDefaultAsync();

            if (account == null)
            {
                ModelState.AddModelError(" ", "Geçersiz e-posta adresi veya şifre.");
                return View(loginModel);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Email)
                
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("AccountInfo", "Authentication"); 
        }
        public async Task<IActionResult> Update()
        {
            var accountEmail = User.Identity.Name;
            var account = await _applicationDbContext.Accounts.Where(a => a.Email == accountEmail).FirstOrDefaultAsync();

            if (account != null)
            {
                var accountUpdateModel = new AccountUpdateModel
                {
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Email = account.Email,
                    Phone = account.Phone
                };

                return View(accountUpdateModel);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Update(AccountUpdateModel accountUpdateModel)
        {
            if (ModelState.IsValid)
            {
                var accountEmail = User.Identity.Name;
                var account = await _applicationDbContext.Accounts.Where(a => a.Email == accountEmail).FirstOrDefaultAsync();

                if (account != null)
                {
                    account.Email = accountUpdateModel.Email;
                    account.FirstName = accountUpdateModel.FirstName;
                    account.LastName = accountUpdateModel.LastName;
                    account.Phone = accountUpdateModel.Phone;
                    account.ModifiedDate = DateTime.UtcNow;

                    _applicationDbContext.Accounts.Update(account);
                    await _applicationDbContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Hesap bilgileri başarıyla güncellendi.";
                    return RedirectToAction("AccountInfo", "Authentication");
                }
            }

            return View(accountUpdateModel);
        }
        public IActionResult UpdatePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel updatePasswordModel)
        {
            if (ModelState.IsValid)
            {
                var accountEmail = User.Identity.Name;
                var account = await _applicationDbContext.Accounts.Where(a => a.Email == accountEmail).FirstOrDefaultAsync();

                if (account != null)
                {
                    if (account.Password != updatePasswordModel.OldPassword)
                    {
                        ModelState.AddModelError("OldPassword", "Eski şifre yanlış.");
                        return View(updatePasswordModel);
                    }

                    account.Password = updatePasswordModel.NewPassword;
                    account.ModifiedDate = DateTime.UtcNow;

                    _applicationDbContext.Accounts.Update(account);
                    await _applicationDbContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Şifre başarıyla güncellendi.";
                    return RedirectToAction("AccountInfo", "Authentication");
                }
            }

            return View(updatePasswordModel);
        }

        public async Task<IActionResult> AccountInfo()
        {
            var accountEmail = User.Identity.Name;
            
            var account = await _applicationDbContext.Accounts.Where(a => a.Email == accountEmail).FirstOrDefaultAsync();

            if (account != null)
            {
                var AccountInfoModel = new AccountInfoModel
                {
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Email = account.Email,
                    Phone = account.Phone
                };

                return View(AccountInfoModel);
            }

            return RedirectToAction("Index", "Home");
        }

    }
}

