using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MyProject.Data;
using MyProject.Models;
using MyProject.Service;
using MyProject.Utilities.Email;

namespace MyProject.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly AccountService _accountService;
        private readonly ApplicationDbContext _applicationDbContext;
        public AuthenticationController(ApplicationDbContext applicationDbContext, IDistributedCache distributedCache, EmailService emailService, AccountService accountService)
        {
            _applicationDbContext = applicationDbContext;
            _accountService = accountService;
        }
        #region Get Index
        public IActionResult Index()
        {
            return View();
        }
        #endregion
        #region Get Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        #endregion
        #region Get Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Cities = await _applicationDbContext.Cities.ToListAsync();
            return View();
        }
        #endregion
        #region Get Update
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            var accountEmail = User.Identity.Name;
            
            var accountUpdateModel = await _accountService.GetAccountForUpdateAsync(accountEmail);

            if (accountUpdateModel != null)
            {
                var cities = await _applicationDbContext.Cities.ToListAsync();
                ViewBag.Cities = cities;
                return View(accountUpdateModel);
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion
        #region Get AccountInfo
        [HttpGet]
        public async Task<IActionResult> AccountInfo()
        {
            var accountEmail = User.Identity.Name;
            var accountInfoModel = await _accountService.GetAccountInfoAsync(accountEmail);  // _accountService bir IAccountService tipinde bir field veya property olmalı.

            if (accountInfoModel != null)
            {
                return View(accountInfoModel);
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion
        #region Get ValidateTokenCallBack
        [HttpGet]
        public async Task<IActionResult> ValidateTokenCallBack(string validationToken)
        {
            if (string.IsNullOrEmpty(validationToken))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _accountService.ValidateAndActivateAccountAsync(validationToken); // _accountService, IAccountService tipinde bir field olmalıdır ve constructor üzerinden enjekte edilmelidir.

            if (!result)
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["SuccessMessage"] = "Hesabınız başarıyla onaylandı.";
            return RedirectToAction("Login", "Authentication");
        }
        #endregion
        #region Get UpdatePassword
        [HttpGet]
        public IActionResult UpdatePassword()
        {
            return View();
        }
        #endregion
        #region Get DeleteConfirmation
        [HttpGet]
        public IActionResult DeleteConfirmation()
        {
            return View(); 
        }
        #endregion
        #region Post Register
        [HttpPost]
        public async Task<IActionResult> Register(AccountModel accountModel)
        {
            var existingAccount = await _applicationDbContext.Accounts
                .Where(a => a.Email == accountModel.Email || a.Phone == accountModel.Phone)
                .FirstOrDefaultAsync();

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
            }

            if (accountModel.Password != accountModel.PasswordAgain)
            {
                ModelState.AddModelError("Password", "Parola eşleşmiyor. Lütfen tekrar deneyin.");
            }

            if (ModelState.IsValid)
            {
                bool result = await _accountService.CreateAsync(accountModel);

                if (result)
                {
                    TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            ViewBag.Cities = await _applicationDbContext.Cities.ToListAsync();
            return View(accountModel);
        }
        #endregion
        #region Post Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }

            try
            {
                var accountModel = await _accountService.LoginAsync(loginModel);

                if (accountModel == null)
                {
                    ModelState.AddModelError("", "Geçersiz e-posta adresi veya şifre.");
                    return View(loginModel);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, accountModel.Email),
                    new Claim(ClaimTypes.Name, accountModel.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("AccountInfo", "Authentication");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(loginModel);
            }
        }
        #endregion
        #region Post Update
        [HttpPost]
        public async Task<IActionResult> Update(AccountUpdateModel accountUpdateModel)
        {
            if (ModelState.IsValid)
            {
                var accountEmail = User.Identity.Name;
                
                var updateSuccessful = await _accountService.UpdateAccountAsync(accountUpdateModel, accountEmail);

                if (updateSuccessful)
                {
                    TempData["SuccessMessage"] = "Hesap bilgileri başarıyla güncellendi.";
                    return RedirectToAction("AccountInfo", "Authentication");
                }
                else
                {
                    ModelState.AddModelError("", "Hesap güncellenemedi.");
                }
            }

            var cities = await _applicationDbContext.Cities.ToListAsync();
            ViewBag.Cities = cities;
            return View(accountUpdateModel);
        }
        #endregion
        #region Post UpdatePassword
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel updatePasswordModel)
        {
            if (ModelState.IsValid)
            {
                var accountEmail = User.Identity.Name;
                
                var updateSuccessful = await _accountService.UpdatePasswordAsync(updatePasswordModel, accountEmail);

                if (updateSuccessful)
                {
                    TempData["SuccessMessage"] = "Şifre başarıyla güncellendi.";
                    return RedirectToAction("AccountInfo", "Authentication");
                }
                else
                {
                    ModelState.AddModelError("OldPassword", "Eski şifre yanlış.");
                }
            }
            return View(updatePasswordModel);
        }
        #endregion
        #region Post delete
        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            var accountEmail = User.Identity.Name;
            
            var deleteSuccessful = await _accountService.DeleteAccountAsync(accountEmail);

            if (deleteSuccessful)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["SuccessMessage"] = "Hesap başarıyla silindi.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}