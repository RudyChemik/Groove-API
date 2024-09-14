using Groove.Interfaces;
using Groove.Models;
using Groove.VM;
using Groove.VM.Artist;
using Groove.VM.Studio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for managing account-related operations
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IArtistService _artistService;
        private readonly IStudioService _studioService;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="accountService">Service for handling account-specific operations</param>
        /// <param name="artistService">Service for handling artist-related operations</param>
        /// <param name="studioService">Service for handling studio-related operations</param>
        public AccountController(IAccountService accountService, IArtistService artistService, IStudioService studioService)
        {
            _accountService = accountService;
            _artistService = artistService;
            _studioService = studioService;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="registerVM">The registration view model containing user information.</param>
        /// <returns>Returns Ok if registration is successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterVM registerVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var emailExist = await _accountService.FindUserByEmailAsync(registerVM.Email);
                    if (emailExist)
                    {
                        return StatusCode(400, "Given e-mail is already in use.");
                    }

                    var any = await _accountService.AnyUsers();
                    if (any == false)
                    {
                        var head = await _accountService.RegisterAsHead(registerVM);
                        if (head.Succeeded) return Ok(head);

                        return StatusCode(400, head.Errors);
                    }

                    var res = await _accountService.RegisterUser(registerVM);

                    if (res.Succeeded)
                        return Ok(res);

                    return StatusCode(400, res.Errors);
                }

                return StatusCode(400, "Invalid model state.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Logs in a user
        /// </summary>
        /// <param name="loginVM">The login view model containing user credentials</param>
        /// <returns>Returns a JWT token if login is successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loginResponse = await _accountService.LoginUser(loginVM);

                    if (loginResponse.IdentityResult.Succeeded)
                    {
                        return Ok(new { token = loginResponse.JwtToken });
                    }

                    return StatusCode(400, new { errors = loginResponse.IdentityResult.Errors });
                }

                return StatusCode(400, "Invalid email address.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Saves user information.
        /// </summary>
        /// <param name="userId">The ID of the user to save information for</param>
        /// <param name="userInfo">The user information to save</param>
        /// <returns>Returns Ok if the information is saved successfully; otherwise, returns BadRequest on error</returns>
        [HttpPost("{userId}")]
        [Authorize]
        public async Task<IActionResult> SaveUserInformation(string userId, [FromBody] UserInfoVM userInfo)
        {
            if (await _accountService.SaveUserInformation(userId, userInfo))
            {
                return Ok();
            }
            else
            {
                return StatusCode(400, "Couldn't save user information.");
            }
        }

        /// <summary>
        /// Changes the user's password
        /// </summary>
        /// <param name="model">The model containing the current and new passwords</param>
        /// <returns>Returns Ok if the password is changed successfully; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassVM model)
        {
            try
            {
                var res = await _accountService.ChangePassword(model);

                if (res.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(400, res.Errors);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Registers a new user as an artist
        /// </summary>
        /// <param name="model">The registration model containing artist information</param>
        /// <returns>Returns Ok if registration is successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegsiterAsArtist([FromBody] RegisterAsArtistVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var emailExist = await _accountService.FindUserByEmailAsync(model.Email);
                    if (emailExist)
                    {
                        return StatusCode(400, "Email already in use");
                    }

                    var registerVM = new RegisterVM
                    {
                        Name = model.Email,
                        Email = model.Email,
                        Password = model.Password
                    };

                    var reg = await _accountService.RegisterUser(registerVM);

                    if (reg.Succeeded)
                    {
                        var user = await _accountService.GetUserIdByEmail(model.Email);

                        if (user == null) { return BadRequest(); }

                        var artistModel = new BecomeArtistVM
                        {
                            Name = model.Name,
                        };

                        var res = await _artistService.BecomeArtistAsync(artistModel, user);

                        if (res)
                            return Ok(res);

                        return StatusCode(400, res);
                    }
                    return StatusCode(400, "Failed to register user as artist.");
                }
                return StatusCode(400, "Invalid request model");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Registers a new user as a studio
        /// </summary>
        /// <param name="model">The registration model containing studio information</param>
        /// <returns>Returns Ok if registration is successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsStudio([FromBody] RegisterAsStudioVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var emailExist = await _accountService.FindUserByEmailAsync(model.Email);
                    if (emailExist)
                    {
                        return BadRequest("Email already exists");
                    }

                    var registerVM = new RegisterVM
                    {
                        Name = model.Email,
                        Email = model.Email,
                        Password = model.Password
                    };

                    var reg = await _accountService.RegisterUser(registerVM);

                    if (reg.Succeeded)
                    {
                        var user = await _accountService.GetUserIdByEmail(model.Email);

                        if (user == null) { return BadRequest(); }

                        var studioModel = new CreateStudioVM
                        {
                            Name = model.Name,
                        };

                        var res = await _studioService.CreateStudio(studioModel, user);

                        if (res)
                            return Ok();

                        return StatusCode(400, "Failed to register user as studio.");
                    }
                    return StatusCode(400, "Failed to register user.");
                }
                return StatusCode(400, "Invalid request model");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
