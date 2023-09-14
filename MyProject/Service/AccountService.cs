using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MyProject.Data;
using MyProject.Interface;
using MyProject.Models;
using MyProject.Utilities.Security.Hashing;
using Newtonsoft.Json;

namespace MyProject.Service
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly EmailService _emailService;
        private readonly IDistributedCache _distributedCache; 

        public AccountService(ApplicationDbContext applicationDbContext, EmailService emailService,IDistributedCache distributedCache)
        {
            _applicationDbContext = applicationDbContext;
            _emailService = emailService;
            _distributedCache = distributedCache;
        }

        #region CreateAsync
        public async Task<(bool, string)> CreateAsync(AccountModel accountModel)
        {
            var existingAccount = await _applicationDbContext.Accounts
                                    .Where(a => a.Email == accountModel.Email || a.Phone == accountModel.Phone)
                                    .FirstOrDefaultAsync();

            if (existingAccount != null)
            {
                if (existingAccount.Email == accountModel.Email || existingAccount.Phone == accountModel.Phone)
                {
                    return (false, "This email address is already in use or This phone number is already in use. Or both ");
                }

            }

            if (accountModel.Password != accountModel.PasswordAgain)
            {
                return (false, "Password does not match. Please try again.");
            }

            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(accountModel.Password, out passwordHash, out passwordSalt);

            var account = new Account
            {
                FirstName = accountModel.FirstName,
                LastName = accountModel.LastName,
                Email = accountModel.Email,
                Phone = accountModel.Phone,
                CityId = accountModel.CityId, 
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDate = DateTime.UtcNow  
            };

            await _applicationDbContext.Accounts.AddAsync(account);
            var result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
            {
                await _emailService.SendValidationEmailAsync(accountModel);
                return (true, "The account has been created successfully.");
            }

            return (false, "The account could not be created.");
        }

        #endregion

        #region loginAsync
        public async Task<AccountModel> LoginAsync(LoginModel loginModel)
        {
            // Cache'de bu e-posta adresi ile bir hesap var mı kontrol et
            var cachedAccount = await _distributedCache.GetStringAsync($"account:{loginModel.Email}");
            Account account = null;

            if (cachedAccount == null)
            {
                // Veritabanında hesabın olup olmadığını kontrol et
                account = await _applicationDbContext.Accounts
                            .Where(a => a.Email == loginModel.Email)
                            .FirstOrDefaultAsync();

                // Eğer hesap bulunamazsa ya da şifre doğru değilse
                if (account == null || !HashingHelper.VerifyPasswordHash(loginModel.Password, account.PasswordHash, account.PasswordSalt))
                {
                    return null;  // Başarısız giriş
                }

                // Hesap aktif değilse hata fırlat
                if (!account.IsActive)
                {
                    throw new Exception("Account is inactive. Your account must be active to log in.");
                }

                // Kullanıcı bilgilerini cache'e ekle
                var serializedData = JsonConvert.SerializeObject(account);
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };
                await _distributedCache.SetStringAsync($"account:{loginModel.Email}", serializedData, cacheEntryOptions);
            }
            else
            {
                account = JsonConvert.DeserializeObject<Account>(cachedAccount);

                // Şifre kontrolü yap, eğer yanlışsa null döndür
                if (!HashingHelper.VerifyPasswordHash(loginModel.Password, account.PasswordHash, account.PasswordSalt))
                {
                    return null;  // Başarısız giriş
                }

                // Hesap aktif değilse hata fırlat
                if (!account.IsActive)
                {
                    throw new Exception("Account is inactive. Your account must be active to log in.");
                }
            }

            // Başarılı giriş için modeli oluştur
            return new AccountModel
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                Phone = account.Phone,
                CityId = account.CityId,
                IsActive = account.IsActive
            };
        }


        #endregion

        #region UpdateAccountAsync
        public async Task<(bool, string)> UpdateAccountAsync(AccountUpdateModel accountUpdateModel, string accountEmail)
        {
            var account = await _applicationDbContext.Accounts
                                .Where(a => a.Email == accountEmail)
                                .FirstOrDefaultAsync();

            if (account == null)
            {
                return (false, "Account not found.");
            }

            var existingAccountWithPhone = await _applicationDbContext.Accounts
                                .Where(a => a.Phone == accountUpdateModel.Phone && a.Email != accountEmail)
                                .FirstOrDefaultAsync();
                                
            if(existingAccountWithPhone != null)
            {
                return (false, "Phone number is used.");
            }

            account.FirstName = accountUpdateModel.FirstName;
            account.LastName = accountUpdateModel.LastName;
            account.Phone = accountUpdateModel.Phone;
            account.CityId = accountUpdateModel.CityId;
            account.ModifiedDate = DateTime.UtcNow;

            _applicationDbContext.Accounts.Update(account);
            await _applicationDbContext.SaveChangesAsync();

            var serializedData = JsonConvert.SerializeObject(account);
            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            await _distributedCache.SetStringAsync($"account:{accountEmail}", serializedData, cacheEntryOptions);

            return (true, "Account updated");
        }


        #endregion

        #region UpdatePasswordAsync
        public async Task<(bool,string)> UpdatePasswordAsync(UpdatePasswordModel updatePasswordModel, string accountEmail)
        {
            var account = await _applicationDbContext.Accounts
                        .Where(a => a.Email == accountEmail)
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                return (false, "Account not found.");
            }
            if (HashingHelper.VerifyPasswordHash(updatePasswordModel.NewPassword, account.PasswordHash, account.PasswordSalt))
            {
                return (false, "The new password cannot be the same as the old password.");
            }
            if (updatePasswordModel.NewPassword != updatePasswordModel.ConfirmNewPassword)
            {
                return (false, "The new password and password verification do not match.");
            }
            if (!HashingHelper.VerifyPasswordHash(updatePasswordModel.OldPassword, account.PasswordHash, account.PasswordSalt))
            {
                return (false, "The old password is incorrect.");
            }

            byte[] newPasswordHash, newPasswordSalt;
            HashingHelper.CreatePasswordHash(updatePasswordModel.NewPassword, out newPasswordHash, out newPasswordSalt);

            account.PasswordHash = newPasswordHash;
            account.PasswordSalt = newPasswordSalt;
            account.ModifiedDate = DateTime.UtcNow;

            _applicationDbContext.Accounts.Update(account);

            await _applicationDbContext.SaveChangesAsync();
            var serializedData = JsonConvert.SerializeObject(account);
            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            await _distributedCache.SetStringAsync($"account:{accountEmail}", serializedData, cacheEntryOptions);
            return (true, "Password has been updated successfully.");        
        }
        #endregion

        #region DeleteAccountAsync
        public async Task<bool> DeleteAccountAsync(string accountEmail)
        {
            var account = await _applicationDbContext.Accounts
                            .Where(a => a.Email == accountEmail)
                            .FirstOrDefaultAsync();
            var accountToken = await _applicationDbContext.RegisterTokens
                            .Where(p => p.Email == accountEmail)
                            .FirstOrDefaultAsync();
            
            if (account == null || accountToken == null)
            {
                return false;
            }

            _applicationDbContext.Accounts.Remove(account);
            _applicationDbContext.RegisterTokens.Remove(accountToken);
            await _applicationDbContext.SaveChangesAsync();
            
            await _distributedCache.RemoveAsync($"account:{accountEmail}");

            return true;
        }
        #endregion

        #region GetAccountForUpdateAsync       
        public async Task<AccountUpdateModel> GetAccountForUpdateAsync(string accountEmail)
        {
            var cacheKey = $"account:{accountEmail}";
            var cachedAccount = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedAccount != null)
            {
                var account = JsonConvert.DeserializeObject<AccountModel>(cachedAccount);
                var accountUpdateModel = new AccountUpdateModel
                {
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    CityId = account.CityId,
                    Phone = account.Phone
                };
                return accountUpdateModel;
            }
            
            return null;
        }
        #endregion

        #region ValidateAndActivateAccountAsync
        public async Task<bool> ValidateAndActivateAccountAsync(string validationToken)
        {
            var registerToken = await _applicationDbContext.RegisterTokens
                                    .Where(rt => rt.Token == validationToken && rt.Expires > DateTime.UtcNow)
                                    .FirstOrDefaultAsync();

            if (registerToken == null)
            {
                return false;
            }

            var account = await _applicationDbContext.Accounts
                                .Where(a => a.Email == registerToken.Email)
                                .FirstOrDefaultAsync();

            if (account == null)
            {
                return false;
            }

            account.IsActive = true;
            await _applicationDbContext.SaveChangesAsync();

            return true;
        }
        #endregion

        #region GetAccountInfoAsync
        public async Task<AccountInfoModel> GetAccountInfoAsync(string accountEmail)
        {
            var cacheKey = $"account:{accountEmail}";
            var cachedAccount = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedAccount == null)
            {
                return null;
            }

            var account = JsonConvert.DeserializeObject<AccountModel>(cachedAccount);
            var cities = await _applicationDbContext.Cities.ToListAsync();

            var cityName = cities.FirstOrDefault(c => c.CityId == account.CityId)?.CityName;
            if (cityName == null)
            {
                cityName = "unknown city";
            }

            var accountInfoModel = new AccountInfoModel
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                CityId = account.CityId,
                CityName = cityName,
                Phone = account.Phone
            };

            return accountInfoModel;
        }
        #endregion
        
        #region GetSortedCitiesAsync
        public async Task<List<City>> GetSortedCitiesAsync()
        {
            return await _applicationDbContext.Cities
                        .OrderBy(p => p.CityName)
                        .ToListAsync();
        }
        #endregion
    }
}