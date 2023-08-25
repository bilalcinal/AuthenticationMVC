using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MyProject.Data;
using MyProject.Models;
using MyProject.Utilities.Security.Hashing;
using Newtonsoft.Json;

namespace MyProject.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDistributedCache _distributedCache;
        public AuthenticationController(ApplicationDbContext applicationDbContext, IDistributedCache distributedCache)
        {
            _applicationDbContext = applicationDbContext;
            _distributedCache = distributedCache;
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

                await _applicationDbContext.SaveChangesAsync();

                var cacheKey = $"account:{accountModel.Email}";
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
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

            if (cachedAccount == null)
            {
                ModelState.AddModelError("", "Geçersiz e-posta adresi veya şifre.");
                return View(loginModel);
            }

            var accountWithoutPassword = JsonConvert.DeserializeObject<AccountModel>(cachedAccount);
            var account = await _applicationDbContext.Accounts
                .Where(a => a.Email == loginModel.Email)
                .FirstOrDefaultAsync();

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