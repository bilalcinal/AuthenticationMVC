﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MyProject.Data;
using MyProject.Interface;
using MyProject.Models;
using MyProject.Service;

namespace MyProject.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAccountService _accountService;
        public AuthenticationController(ApplicationDbContext applicationDbContext, IDistributedCache distributedCache, EmailService emailService, IAccountService accountService)
        {
            _accountService = accountService;
        }

        #region  Index
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region Register

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Cities = await _accountService.GetSortedCitiesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(AccountModel accountModel)
        {
            try
            {
                if (ModelState.IsValid)
                    {
                        var (result, message) = await _accountService.CreateAsync(accountModel);

                        if (result)
                        {
                            TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                            return RedirectToAction("Login", "Authentication");
                        }
                        else
                        {
                            ModelState.AddModelError("", message);
                        }
                    }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            ViewBag.Cities = await _accountService.GetSortedCitiesAsync();
            return View(accountModel);
        }
        #endregion

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }

            try
            {
                // LoginAsync içinde işlemler yapılıyor
                var accountModel = await _accountService.LoginAsync(loginModel);

                // Eğer hesap modeli null ise şifre veya e-posta yanlış
                if (accountModel == null)
                {
                    ModelState.AddModelError("", "Geçersiz e-posta adresi veya şifre.");
                    return View(loginModel);
                }

                // Kullanıcının claim'lerini oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, accountModel.Email),
                    new Claim(ClaimTypes.Name, accountModel.Email)
                };

                // Kullanıcı kimliğini oluştur
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Oturum açma özelliklerini ayarla
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                // Kullanıcıyı oturum açtır
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

        #region Update
        [HttpGet]
        public async Task<IActionResult> Update()
        {

            var accountEmail = User.Identity.Name;

            var accountUpdateModel = await _accountService.GetAccountForUpdateAsync(accountEmail);

            if (accountUpdateModel != null)
            {
                ViewBag.Cities = await _accountService.GetSortedCitiesAsync();
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

                var (updateSuccessful, message) = await _accountService.UpdateAccountAsync(accountUpdateModel, accountEmail);

                if (updateSuccessful)
                {
                    TempData["SuccessMessage"] = "Hesap bilgileri başarıyla güncellendi.";
                    return RedirectToAction("AccountInfo", "Authentication");
                }
                else
                {
                    ModelState.AddModelError("", message);
                }
            }

            ViewBag.Cities = await _accountService.GetSortedCitiesAsync();
            return View(accountUpdateModel);
        }

        #endregion

        #region UpdatePassword
        [HttpGet]
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

        #region AccountInfo
        [HttpGet]
        public async Task<IActionResult> AccountInfo()
        {
            var accountEmail = User.Identity.Name;
            var accountInfoModel = await _accountService.GetAccountInfoAsync(accountEmail);

            if (accountInfoModel != null)
            {
                return View(accountInfoModel);
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region ValidateTokenCallBack
        [HttpGet]
        public async Task<IActionResult> ValidateTokenCallBack(string validationToken)
        {
            if (string.IsNullOrEmpty(validationToken))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _accountService.ValidateAndActivateAccountAsync(validationToken);
            if (!result)
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["SuccessMessage"] = "Hesabınız başarıyla onaylandı.";
            return RedirectToAction("Login", "Authentication");
        }
        #endregion

        #region delete

        [HttpGet]
        public IActionResult DeleteConfirmation()
        {
            return View();
        }

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