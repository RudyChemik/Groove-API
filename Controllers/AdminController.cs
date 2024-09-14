using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.Interfaces;
using Groove.Models.App;
using Groove.VM.App;
using Groove.VMO.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for handling administrative operations such as managing application status, updating page data, and managing users.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IBlobService _blobService;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="adminService">Service for handling admin-specific operations</param>
        /// <param name="blobService">Service for managing blob storage</param>
        public AdminController(IAdminService adminService, IBlobService blobService)
        {
            _adminService = adminService;
            _blobService = blobService;
        }

        /// <summary>
        /// Retrieves the current status of the application (open or closed).
        /// </summary>
        /// <returns>Returns true if the application is closed; otherwise, false</returns>
        [HttpGet]
        public ActionResult<bool> GetAppStatus()
        {
            try
            {
                var isClosed = _adminService.IsAppClosed();
                return Ok(isClosed);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        /// <returns>Returns Ok if the operation is successful; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public ActionResult CloseApp()
        {
            try
            {
                _adminService.SetAppStatus(true);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens the application
        /// </summary>
        /// <returns>Returns Ok if the operation is successful; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public ActionResult OpenApp()
        {
            try
            {
                _adminService.SetAppStatus(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<AppRes>> GetAppData()
        {
            try
            {
                var appData = await _adminService.GetAppData();
                return Ok(appData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the page data of the application
        /// </summary>
        /// <param name="appResViewModel">The view model containing the page data to updat</param>
        /// <returns>Returns Ok if the update is successful; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<ActionResult> UpdatePageData([FromForm] AppResVM appResViewModel)
        {
            try
            {
                string imageUrl = "";
                if (appResViewModel.ImgFile != null && appResViewModel.ImgFile.Length > 0)
                {
                    imageUrl = await _blobService.UploadBlobMainPic(appResViewModel.ImgFile);
                }

                var appRes = new AppRes
                {
                    Name = appResViewModel.Name,
                    Img = imageUrl
                };

                await _adminService.UpdatePageData(appRes);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all users of the application
        /// </summary>
        /// <returns>A list of all users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserVMO>>> GetAllUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Assigns a new head admin
        /// </summary>
        /// <param name="newadminId">The ID of the new head admin to assign</param>
        /// <returns>Returns Ok if the operation is successful; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<IActionResult> MainNewHeadAdmin(string newadminId)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (identity == null)
                    return StatusCode(400, "User identity not found");

                var userIdClaim = identity.FindFirst("UserId");
                if (userIdClaim == null)
                    return StatusCode(400, "UserId claim not found");

                string userId = userIdClaim.Value;

                var adm = await _adminService.BecomeHead(newadminId);
                if (adm == false)
                    return StatusCode(400, "Failed to assign new admin");

                var remove = await _adminService.RemoveOldHead(userId);
                if (remove == false)
                    return StatusCode(400, "Failed to remove old admin");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }
    }
}
