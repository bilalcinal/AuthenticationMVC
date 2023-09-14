using System.Threading.Tasks;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Interface
{
    public interface IAccountService
    {
        Task<(bool,string)> CreateAsync(AccountModel accountModel);
        Task<AccountModel> LoginAsync(LoginModel loginModel);
        Task<(bool, string)> UpdateAccountAsync(AccountUpdateModel accountUpdateModel, string accountEmail);
        Task<(bool,string)> UpdatePasswordAsync(UpdatePasswordModel updatePasswordModel, string accountEmail);       
        Task<bool> DeleteAccountAsync(string accountEmail);
        Task<AccountUpdateModel> GetAccountForUpdateAsync(string accountEmail);
        Task<bool> ValidateAndActivateAccountAsync(string validationToken);
        Task<AccountInfoModel> GetAccountInfoAsync(string accountEmail);
        Task<List<City>> GetSortedCitiesAsync();

    }
}
