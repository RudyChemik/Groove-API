using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.VM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Groove.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AccountService(UserManager<AppUser> userManager, IConfiguration configuration, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = appDbContext;
        }
        public async Task<bool> FindUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<IdentityResult> RegisterUser(RegisterVM model)
        {
            var user = new AppUser { UserName = model.Email, Email = model.Email, Name = model.Name };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
            }

            return result;
        }

        public async Task<IdentityResult> RegisterAsHead(RegisterVM model)
        {
            var user = new AppUser { UserName = model.Email, Email = model.Email, Name = model.Name };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                await _userManager.AddToRoleAsync(user, "admin");
            }

            return result;
        }


        public async Task<LoginResponse> LoginUser(LoginVM loginVM)
        {
            var user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user == null)
            {
                return new LoginResponse
                {
                    IdentityResult = IdentityResult.Failed(new IdentityError { Description = "Niepoprawny adres email" })
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, loginVM.Password);

            if (!result)
            {
                return new LoginResponse
                {
                    IdentityResult = IdentityResult.Failed(new IdentityError { Description = "Niepoprawne hasło" })
                };
            }


            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim("Email", loginVM.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("UserId", user.Id),
            };


            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            var tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponse
            {
                IdentityResult = IdentityResult.Success,
                JwtToken = tokenAsString
            };
        }

        public async Task<bool> SaveUserInformation(string userId, UserInfoVM userInfo)
        {
            if(userInfo == null)
            {
                return false;
            }

            var existingInfo = await _context.UserInformation.FirstOrDefaultAsync(a => a.UserId == userId);

            if(existingInfo == null)
            {
                var newInfo = new UserInformation
                {
                    UserId = userId,
                    Street = userInfo.Street,
                    City = userInfo.City,
                    Country = userInfo.Country,
                    PostalCode = userInfo.PostalCode,

                };
                _context.UserInformation.Add(newInfo);
            }
            else
            {
                existingInfo.UserId = userId;
                existingInfo.Street = userInfo.Street;
                existingInfo.City = userInfo.City;
                existingInfo.Country = userInfo.Country;
                existingInfo.PostalCode = userInfo.PostalCode;

            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IdentityResult> ChangePassword(ChangePassVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            if(model.NewPassword != model.ConfirmNewPassword)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Password doesn ot match" });
            }

            var res = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            return res;
        }

        public async Task<string> ReturnJWT(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {                
                return null;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("UserId", user.Id),
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            var tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenAsString;
        }

        public async Task<string> GetUserIdByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user.Id;
        }

        public async Task<bool> AnyUsers()
        {
            return await _userManager.Users.AnyAsync();
        }
    }
}
