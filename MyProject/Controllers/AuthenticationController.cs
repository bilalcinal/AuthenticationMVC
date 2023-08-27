using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MyProject.Data;
using MyProject.Models;
using MyProject.Utilities.Email;
using MyProject.Utilities.Security.Hashing;
using MyProject.Utilities.Token;
using Newtonsoft.Json;

namespace MyProject.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDistributedCache _distributedCache;
        private readonly EmailService _emailService;
        public AuthenticationController(ApplicationDbContext applicationDbContext, IDistributedCache distributedCache, EmailService emailService)
        {
            _applicationDbContext = applicationDbContext;
            _distributedCache = distributedCache;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
          
            ViewBag.Cities = await _applicationDbContext.Cities.ToListAsync();
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            var accountEmail = User.Identity.Name;

            var cacheKey = $"account:{accountEmail}";
            var cachedAccount = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedAccount != null)
            {
                var account = JsonConvert.DeserializeObject<AccountModel>(cachedAccount);

                var cities = await _applicationDbContext.Cities.ToListAsync();
                var accountUpdateModel = new AccountUpdateModel
                {
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    CityId = account.CityId,
                    Phone = account.Phone
                };
                ViewBag.Cities = cities;

                return View(accountUpdateModel);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> AccountInfo()
        {
            var accountEmail = User.Identity.Name;
            var cacheKey = $"account:{accountEmail}";
            var cachedAccount = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedAccount != null)
            {
                var account = JsonConvert.DeserializeObject<AccountModel>(cachedAccount);
                var cities = await _applicationDbContext.Cities.ToListAsync();

                var cityName = cities.FirstOrDefault(c => c.CityId == account.CityId)?.CityName;
                if (cityName == null)
                {
                    cityName = "Unknown City";
                }

                var AccountInfoModel = new AccountInfoModel
                {
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Email = account.Email,
                    CityId = account.CityId,
                    CityName = cityName, 
                    Phone = account.Phone
                };

                return View(AccountInfoModel);
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult ValidateTokenCallback(string validationToken)
        {
            if (string.IsNullOrEmpty(validationToken))
            {
                return RedirectToAction("Index", "Home");
            }

            var registerToken = _applicationDbContext.RegisterTokens
                .Where(rt => rt.Token == validationToken && rt.Expires > DateTime.UtcNow)
                .FirstOrDefault();

            if (registerToken == null)
            {
                return RedirectToAction("Index", "Home"); 
            }

            var account = _applicationDbContext.Accounts
                .Where(a => a.Email == registerToken.Email)
                .FirstOrDefault();

            if (account == null)
            {
                return RedirectToAction("Index", "Home"); 
            }

            account.IsActive = true; 
            _applicationDbContext.SaveChanges(); 

            TempData["SuccessMessage"] = "Hesabınız başarıyla onaylandı.";
            return RedirectToAction("Login", "Authentication"); 
        }
        [HttpGet]
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
                byte[] passwordHash, passwordSalt;
                HashingHelper.CreatePasswordHash(accountModel.Password, out passwordHash, out passwordSalt);

                _applicationDbContext.Accounts.Add(new Account
                {
                    FirstName = accountModel.FirstName,
                    LastName = accountModel.LastName,
                    Email = accountModel.Email,
                    Phone = accountModel.Phone,
                    CityId = accountModel.CityId,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedDate = DateTime.UtcNow
                });
                var tokenGenerator = new TokenGenerator();
                var token = tokenGenerator.GenerateToken();
                var registerToken = new RegisterToken
                {
                    Email = accountModel.Email,
                    Token = token,
                    Expires = DateTime.UtcNow.AddMinutes(60) 
                };

                _applicationDbContext.Add(registerToken);

                await _applicationDbContext.SaveChangesAsync();
                var validateTokenUrl = Url.Action("ValidateTokenCallback", "Authentication", new { validationToken = token }, Request.Scheme);
                var emailModel = new EmailModel
                {
                    ToEmail = accountModel.Email,
                    Subject = "Hoş Geldiniz!",
                    Body = $@"<!DOCTYPE html>
                                <html lang=""en"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Complete Registration</title>
                                </head>
                                <body>
                                    <h1>Welcome to our platform!</h1>
                                    <p>Thank you for registering. To complete your registration, please click the following link:</p>
                                    <p><a href=""{validateTokenUrl}"">Complete Registration</a></p>
                                    <p>If the link above doesn't work, you can copy and paste the following URL into your browser's address bar:</p>
                                    <p>{validateTokenUrl}</p>
                                    <p>We're excited to have you on board. If you have any questions, feel free to contact us.</p>
                                    <p>Best regards,</p>
                                    <p>Your Team</p>
                                </body>
                                </html>"
                };

                await _emailService.SendEmailAsync(emailModel);

                var cacheKey = $"account:{accountModel.Email}";
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };

                var accountData = new AccountModel
                {
                    FirstName = accountModel.FirstName,
                    LastName = accountModel.LastName,
                    Email = accountModel.Email,
                    Phone = accountModel.Phone,
                    CityId = accountModel.CityId
                };

                var serializedData = JsonConvert.SerializeObject(accountData); 
                await _distributedCache.SetStringAsync(cacheKey, serializedData, cacheEntryOptions);

                TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                return RedirectToAction("Login", "Authentication");
            }

            ViewBag.Cities = await _applicationDbContext.Cities.ToListAsync();
            return View(accountModel);
        }
    
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var cachedAccount = await _distributedCache.GetStringAsync($"account:{loginModel.Email}");

            AccountModel accountWithoutPassword = null;

            if (cachedAccount == null)
            {
                var account = await _applicationDbContext.Accounts
                    .Where(a => a.Email == loginModel.Email)
                    .FirstOrDefaultAsync();

                if (account != null && HashingHelper.VerifyPasswordHash(loginModel.Password, account.PasswordHash, account.PasswordSalt))
                {
                    if (!account.IsActive)
                    {
                        ModelState.AddModelError("", "Hesap etkin değil. Giriş yapmak için hesabınızın etkin olması gerekmektedir.");
                        return View(loginModel);
                    }

                    accountWithoutPassword = new AccountModel
                    {
                        FirstName = account.FirstName,
                        LastName = account.LastName,
                        Email = account.Email,
                        Phone = account.Phone,
                        CityId = account.CityId
                    };

                    var serializedData = JsonConvert.SerializeObject(accountWithoutPassword); 
                    var cacheEntryOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                    };
                    await _distributedCache.SetStringAsync($"account:{loginModel.Email}", serializedData, cacheEntryOptions);
                }
                else
                {
                    ModelState.AddModelError("", "Geçersiz e-posta adresi veya şifre.");
                    return View(loginModel);
                }
            }
            else
            {
                accountWithoutPassword = JsonConvert.DeserializeObject<AccountModel>(cachedAccount);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, accountWithoutPassword.Email),
                new Claim(ClaimTypes.Name, accountWithoutPassword.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("AccountInfo", "Authentication");
        }

        [HttpPost]
        public async Task<IActionResult> Update(AccountUpdateModel accountUpdateModel)
        {
            if (ModelState.IsValid)
            {
                var accountEmail = User.Identity.Name;
                var account = await _applicationDbContext.Accounts
                    .Where(a => a.Email == accountEmail)
                    .FirstOrDefaultAsync();

                if (account != null)
                {
                    account.FirstName = accountUpdateModel.FirstName;
                    account.LastName = accountUpdateModel.LastName;
                    account.Phone = accountUpdateModel.Phone;
                    account.CityId = accountUpdateModel.CityId;
                    account.ModifiedDate = DateTime.UtcNow;

                    _applicationDbContext.Accounts.Update(account);
                    await _applicationDbContext.SaveChangesAsync();

                    var cacheKey = $"account:{accountEmail}";
                    var cacheEntryOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                    };
                    var accountData = new AccountModel
                    {
                        FirstName = account.FirstName,
                        LastName = account.LastName,
                        Email = account.Email,
                        Phone = account.Phone,
                        CityId = account.CityId
                    };
                    var serializedData = JsonConvert.SerializeObject(accountData);
                    await _distributedCache.SetStringAsync(cacheKey, serializedData, cacheEntryOptions);

                    TempData["SuccessMessage"] = "Hesap bilgileri başarıyla güncellendi.";
                    return RedirectToAction("AccountInfo", "Authentication");
                }
            }

            var cities = await _applicationDbContext.Cities.ToListAsync();
            ViewBag.Cities = cities;
            return View(accountUpdateModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel updatePasswordModel)
        {
            if (ModelState.IsValid)
            {
                var accountEmail = User.Identity.Name;
                var account = await _applicationDbContext.Accounts
                .Where(a => a.Email == accountEmail)
                .FirstOrDefaultAsync();

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

        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            var accountEmail = User.Identity.Name;
            var account = await _applicationDbContext.Accounts
            .Where(a => a.Email == accountEmail)
            .FirstOrDefaultAsync();

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