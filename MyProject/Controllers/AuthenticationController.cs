using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.Utilities.Security.Hashing;

namespace MyProject.Controllers
{
    //mail regexi
    //telefon numarası 5 ile başlamalı
    //şehir ve ilçeler
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
        
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register(int Id)
        {
            ViewData["Id"] = Id;
            return View();
        }
         public IActionResult UpdatePassword()
        {
            return View();
        }
        [HttpGet]
        public IActionResult DeleteConfirmation()
        {
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
            if(accountModel.Password != accountModel.PasswordAgain)
                {
                    ModelState.AddModelError("Password", "Password eşleşmiyor lütfen bir daha deneyiniz.");
                }

            if (ModelState.IsValid)
            {
               byte[] passwordHash, passwordSalt;

                HashingHelper.CreatePasswordHash(accountModel.Password, out passwordHash, out passwordSalt);

                _applicationDbContext.Accounts.Add(new Account
                {
                FirstName = accountModel.FirstName,
                LastName = accountModel.LastName,
                Email = accountModel.Email,
                Phone = accountModel.Phone,
                PasswordHash = passwordHash, 
                PasswordSalt = passwordSalt,
                CreatedDate = DateTime.UtcNow  
                });

                await _applicationDbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                return RedirectToAction("Login", "Authentication");
            }

            return View(accountModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var account = await _applicationDbContext.Accounts.FirstOrDefaultAsync(a => a.Email == loginModel.Email);

            if (account == null || !HashingHelper.VerifyPasswordHash(loginModel.Password, account.PasswordHash, account.PasswordSalt))
            {
                ModelState.AddModelError("", "Geçersiz e-posta adresi veya şifre.");
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
       
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel updatePasswordModel)
        {
            if (ModelState.IsValid)
            {
                var accountEmail = User.Identity.Name;
                var account = await _applicationDbContext.Accounts.FirstOrDefaultAsync(a => a.Email == accountEmail);

                if (account != null)
                {
                    
                    if (!HashingHelper.VerifyPasswordHash(updatePasswordModel.OldPassword, account.PasswordHash, account.PasswordSalt))
                    {
                        ModelState.AddModelError("OldPassword", "Eski şifre yanlış.");
                        return View(updatePasswordModel);
                    }

                    byte[] newPasswordHash, newPasswordSalt;
                    HashingHelper.CreatePasswordHash(updatePasswordModel.NewPassword, out newPasswordHash, out newPasswordSalt);

                    account.PasswordHash = newPasswordHash;
                    account.PasswordSalt = newPasswordSalt;
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
        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            var accountEmail = User.Identity.Name;
            var account = await _applicationDbContext.Accounts.Where(a => a.Email == accountEmail).FirstOrDefaultAsync();

            if (account != null)
            {
                _applicationDbContext.Accounts.Remove(account);
                await _applicationDbContext.SaveChangesAsync();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                TempData["SuccessMessage"] = "Hesap başarıyla silindi.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home"); 
        }
    }
}