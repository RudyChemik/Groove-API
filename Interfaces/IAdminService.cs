using Groove.Models;
using Groove.Models.App;
using Groove.VMO.Admin;
using Microsoft.AspNetCore.Identity;

namespace Groove.Interfaces
{
    public interface IAdminService
    {
        public bool IsAppClosed();
        public void SetAppStatus(bool isClosed);
        Task UpdatePageData(AppRes appRes);
        Task<AppRes> GetAppData();
        Task<List<UserVMO>> GetAllUsers();
        Task<bool> BecomeHead(string userId);
        Task<bool> RemoveOldHead(string userId);
    }
}
