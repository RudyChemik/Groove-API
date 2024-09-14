using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.Models.App;
using Groove.VMO.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Groove.Services
{
    public class AdminService:IAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AdminService( AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _context = appDbContext;
            _userManager = userManager;
        }
        public bool IsAppClosed()
        {
            var appStatus = _context.AppStatus.FirstOrDefault();
            return appStatus != null && appStatus.Status;
        }

        public void SetAppStatus(bool isClosed)
        {
            var appStatus = _context.AppStatus.FirstOrDefault();

            if (appStatus == null)
            {
                appStatus = new AppStatus();
                _context.AppStatus.Add(appStatus);
            }

            appStatus.Status = isClosed;
            _context.SaveChanges();
        }

        public async Task UpdatePageData(AppRes appRes)
        {
            var existingAppData = await _context.AppRes.FirstOrDefaultAsync();

            if (existingAppData == null)
            {
                existingAppData = new AppRes();
                _context.AppRes.Add(existingAppData);
            }

            existingAppData.Name = appRes.Name;

            if (appRes.Img != null)
            {
                existingAppData.Img = appRes.Img;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<AppRes> GetAppData()
        {
            return await _context.AppRes.FirstOrDefaultAsync();
        }

        public async Task<List<UserVMO>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return users.Select(u => new UserVMO
            {
                UserId = u.Id,
                Email = u.Email
            }).ToList();
        }

        public async Task<bool> BecomeHead(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            await _userManager.AddToRoleAsync(user, "admin");
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveOldHead(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }
            var res = await _userManager.RemoveFromRoleAsync(user, "admin");
            if (!res.Succeeded)
            {
                return false;
            }
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
