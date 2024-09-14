using Groove.Models;
using Groove.VM;
using Microsoft.AspNetCore.Identity;

namespace Groove.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterUser(RegisterVM model);

        Task<bool> FindUserByEmailAsync(string email);
        Task<LoginResponse> LoginUser(LoginVM model);
        Task<bool> SaveUserInformation(string userId, UserInfoVM userInfo);
        Task<IdentityResult> ChangePassword(ChangePassVM model);
        Task<string> ReturnJWT(string userId);
        Task<string> GetUserIdByEmail(string email);
        Task<IdentityResult> RegisterAsHead(RegisterVM model);
        Task<bool> AnyUsers();
    }

}
