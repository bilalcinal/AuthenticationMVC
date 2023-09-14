using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyProject.Interface;
using MyProject.Models;

namespace MyProject.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAccountService _accountService;
        public AuthenticationController(IAccountService accountService)
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
                            TempData["SuccessMessage"] = "Registration completed successfully.";
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
                    ModelState.AddModelError("", "Invalid email address or password.");
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

            return RedirectToAction("AccountInfo", "Authentication");
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
                    TempData["SuccessMessage"] = "Account information has been updated successfully.";
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

                var (updateSuccessful, message) = await _accountService.UpdatePasswordAsync(updatePasswordModel, accountEmail);

                if (updateSuccessful)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("AccountInfo", "Authentication");
                }
                else
                {
                    if (message == "The old password is incorrect.")
                    {
                        ModelState.AddModelError("OldPassword", message);
                    }
                    else if(message == "The new password cannot be the same as the old password.")
                    {
                        ModelState.AddModelError("OldPassword", message);
                        ModelState.AddModelError("NewPassword", message);
                    }
                    else if (message == "The new password and password verification do not match.")
                    {
                        ModelState.AddModelError("NewPassword", message);
                        ModelState.AddModelError("ConfirmNewPassword", message);
                    }
                    else if (message == "Account not found.")
                    {
                        ModelState.AddModelError(string.Empty, message);
                    }
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

            TempData["SuccessMessage"] = "Your account has been successfully confirmed.";
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
                TempData["SuccessMessage"] = "Account deleted successfully.";
                return RedirectToAction("Login", "Authentication");
            }

            return RedirectToAction("Login", "Authentication");
        }
        #endregion
    }
}