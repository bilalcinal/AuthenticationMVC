using System.Threading.Tasks;
using MyProject.Models;

namespace MyProject.Interface
{
    public interface IAccountService
    {
        Task<bool> CreateAsync(AccountModel accountModel);
        //Task<bool> ValidateToken(string email, string token);
        Task<AccountModel> LoginAsync(LoginModel loginModel);
        Task<bool> UpdateAccountAsync(AccountUpdateModel accountUpdateModel, string accountEmail);
        Task<bool> UpdatePasswordAsync(UpdatePasswordModel updatePasswordModel, string accountEmail);
        Task<bool> DeleteAccountAsync(string accountEmail);
        Task<AccountUpdateModel> GetAccountForUpdateAsync(string accountEmail);
        Task<bool> ValidateAndActivateAccountAsync(string validationToken);
        Task<AccountInfoModel> GetAccountInfoAsync(string accountEmail);

    }
}
