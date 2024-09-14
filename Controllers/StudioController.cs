using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.Services;
using Groove.VM;
using Groove.VM.Studio;
using Groove.VMO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for handling studio-related operations such as creating studios, managing artists, albums, and requests, as well as managing studio administrators.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudioController : Controller
    {
        private readonly IStudioService _studioService;
        private readonly IAccountService _accountService;
        private readonly IArtistService _artistService;
        private readonly IBlobService _blobService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudioController"/> class.
        /// </summary>
        /// <param name="studioService">Service for studio operations.</param>
        /// <param name="accountService">Service for account management.</param>
        /// <param name="artistService">Service for artist management.</param>
        /// <param name="blobService">Service for handling blob storage.</param>
        /// <param name="jwtService">Service for handling JWT authentication.</param>
        public StudioController(IStudioService studioService, IAccountService accountService, IArtistService artistService, IBlobService blobService, IJwtService jwtService)
        {
            _accountService = accountService;
            _studioService = studioService;
            _artistService = artistService;
            _blobService = blobService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Creates a new studio for the authenticated user.
        /// </summary>
        /// <param name="model">The studio creation model.</param>
        /// <returns>Returns JWT if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateStudio([FromBody] CreateStudioVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string userId = _jwtService.GetUserIdClaim();

                    var hasStudio = await _studioService.DoesUserHaveStudio(userId);

                    if (hasStudio)
                    {
                        return BadRequest("Studio already exists");
                    }

                    var res = await _studioService.CreateStudio(model, userId);

                    if (res)
                    {
                        var jwt = await _accountService.ReturnJWT(userId);
                        return Ok(jwt);
                    }

                    throw new ApplicationException("Studio creation failed.");
                }
                return BadRequest("Invalid model state.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all available studios.
        /// </summary>
        /// <returns>A list of all studios, or NotFound if none are found.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllStudios()
        {
            try
            {
                var res = await _studioService.GetAllStudios();
                if (res != null)
                {
                    return Ok(res);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all join requests for the studio owned by the authenticated user.
        /// </summary>
        /// <returns>A list of join requests, or BadRequest if user is not an admin.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ShowAllRequests()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var studioId = await _studioService.GetStudioId(userId);
                if (studioId == 0)
                {
                    return BadRequest("User is not an admin");
                }

                var res = await _studioService.GetAllForStudio(studioId);
                if (res == null)
                {
                    return NotFound();
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error : " + ex.Message);
            }
        }

        /// <summary>
        /// Accepts a join request for the studio.
        /// </summary>
        /// <param name="requestId">The ID of the request to accept.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _studioService.DoesUserHaveStudio(userId);
                if (res)
                {
                    var accept = await _studioService.AcceptRequest(requestId, userId);
                    if (accept)
                    {
                        var del = await _studioService.DeleteRequest(requestId);
                        if (del)
                        {
                            return Ok();
                        }
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Declines a join request for the studio.
        /// </summary>
        /// <param name="requestId">The ID of the request to decline.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeclineRequest(int requestId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _studioService.DoesUserHaveStudio(userId);
                if (res)
                {
                    var decline = await _studioService.DeclineRequest(requestId, userId);
                    if (decline)
                    {
                        var del = await _studioService.DeleteRequest(requestId);
                        if (del) return Ok();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Promotes an artist to an admin of the studio.
        /// </summary>
        /// <param name="artistId">The ID of the artist to promote.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeArtistAnAdm(int artistId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var hasStudio = await _studioService.DoesUserHaveStudio(userId);
                if (hasStudio == false) { return BadRequest(); }

                var studioId = await _studioService.GetStudioId(userId);
                if (studioId == 0) { return BadRequest(); }

                var artistInStudio = await _studioService.IsArtistInStudio(studioId, artistId);
                if (artistInStudio == false) { return BadRequest(); }

                var artistUserId = await _artistService.GetArtistIdById(artistId);
                if (artistUserId != null)
                {
                    var res = await _studioService.MakeArtistAnAdmin(studioId, artistUserId);
                    if (res)
                    {
                        if (await _studioService.UpdateJWT(artistUserId)) { return Ok(); }

                        return BadRequest();
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a new album with free tracks for the studio.
        /// </summary>
        /// <param name="model">The album creation model.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAlbumWithTracksFree([FromForm] CreateFreeAlbumVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string userId = _jwtService.GetUserIdClaim();

                    var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                    if (isUserAnAdmin == 0)
                    {
                        return StatusCode(400, "User is not an admin");
                    }

                    var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, model.ArtistId);
                    if (isArtistInStudio == false) { return BadRequest(); }

                    var res = await _studioService.CreateAlbumWithTracksFree(model, isUserAnAdmin);
                    if (res) { return Ok(); }

                    return BadRequest("Some tracks failed to be added.");
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Adds a single free track to the studio.
        /// </summary>
        /// <param name="model">The single track creation model.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddSingleFree([FromForm] CreateSingleFreeVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, model.ArtistId);
                if (isArtistInStudio == false) { return BadRequest(); }

                string blobURL = await _blobService.UploadBlob(model.Mp3File);

                var res = await _studioService.CreateSingleFree(model, isUserAnAdmin, blobURL);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a new album with paid tracks for the studio.
        /// </summary>
        /// <param name="model">The paid album creation model.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAlbumWithTracksPaid([FromForm] CreatePaidAlbumVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, model.ArtistId);
                if (isArtistInStudio == false) { return BadRequest(); }

                var res = await _studioService.CreatePaidAlbumWithTracks(model, isUserAnAdmin);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Adds a single paid track to the studio.
        /// </summary>
        /// <param name="model">The single paid track creation model.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddSinglePaid([FromForm] CreateSinglePaidVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }
                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, model.ArtistId);
                if (isArtistInStudio == false) { return BadRequest(); }

                string blobUrl = await _blobService.UploadBlobPaid(model.Mp3File);

                var res = await _studioService.CreatePaidSingle(model, isUserAnAdmin, blobUrl);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a free album from the studio.
        /// </summary>
        /// <param name="albumId">The ID of the album to delete.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteAlbumFree(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }

                var artistId = await _artistService.GetArtistIdByAlbumId(albumId);
                if (artistId == 0) { return BadRequest(); }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, artistId);
                if (isArtistInStudio == false) { return BadRequest(); }

                var res = await _studioService.DeleteAlbumFree(albumId, isUserAnAdmin);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a free single track from the studio.
        /// </summary>
        /// <param name="trackId">The ID of the track to delete.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteSingleFree(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }

                var artistId = await _artistService.GetArtistIdByTrackId(trackId);
                if (artistId == 0) { return BadRequest(); }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, artistId);
                if (isArtistInStudio == false) { return BadRequest(); }

                var res = await _studioService.DeleteSingleFree(trackId, isUserAnAdmin);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a paid album from the studio.
        /// </summary>
        /// <param name="paidAlbumId">The ID of the paid album to delete.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteAlbumPaid(int paidAlbumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }

                var artistId = await _artistService.GetArtistIdByPaidAlbum(paidAlbumId);
                if (artistId == 0) { return BadRequest(); }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, artistId);
                if (isArtistInStudio == false) { return BadRequest(); }

                var res = await _studioService.DeleteAlbumPaid(paidAlbumId, isUserAnAdmin);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a paid single track from the studio.
        /// </summary>
        /// <param name="trackId">The ID of the paid track to delete.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteSinglePaid(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }

                var artistId = await _artistService.GetArtistIdByPaidTrackId(trackId);
                if (artistId == 0) { return BadRequest(); }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, artistId);
                if (isArtistInStudio == false) { return BadRequest(); }

                var res = await _studioService.DeleteSinglePaid(trackId, isUserAnAdmin);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Removes a free track from a free album.
        /// </summary>
        /// <param name="trackId">The ID of the track to remove.</param>
        /// <param name="albumId">The ID of the album.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveTrackFromAlbumFree(int trackId, int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "User is not an admin");
                }
                var artistIdTrack = await _artistService.GetArtistIdByTrackId(trackId);
                if (artistIdTrack == 0) { return BadRequest(); }

                var artistIdAlbum = await _artistService.GetArtistIdByAlbumId(albumId);
                if (artistIdAlbum == 0) { return BadRequest(); }

                if (artistIdTrack != artistIdAlbum) { return BadRequest(); }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, artistIdTrack);
                if (isArtistInStudio == false) { return BadRequest(); }

                var res = await _studioService.RemoveFromFreeAlbum(trackId, albumId);
                if (res) { return Ok(); }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }

        }

        /// <summary>
        /// Retrieves all administrators of the studio.
        /// </summary>
        /// <returns>A list of studio administrators, or StatusCode 500 on error.</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<StudioAdmin>>> GetAllAdmins()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return StatusCode(400, "user is not an admin");
                }

                var res = await _studioService.GetAllAdmins(isUserAnAdmin);
                return res;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a track from a paid album.
        /// </summary>
        /// <param name="trackId">The ID of the track to delete.</param>
        /// <param name="albumId">The ID of the album.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteTrackFromPaidAlbum(int trackId, int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var isUserAnAdmin = await _studioService.IsUserAnAdmin(userId);
                if (isUserAnAdmin == 0)
                {
                    return BadRequest("PERM");
                }
                var artistIdTrack = await _artistService.GetArtistIdByPaidAlbumTrackId(trackId);
                if (artistIdTrack == 0) { return BadRequest(); }

                var artistIdAlbum = await _artistService.GetArtistIdByPaidAlbum(albumId);
                if (artistIdAlbum == 0) { return BadRequest(); }

                if (artistIdTrack != artistIdAlbum) { return BadRequest(); }

                var isArtistInStudio = await _studioService.IsArtistInStudio(isUserAnAdmin, artistIdTrack);
                if (isArtistInStudio == false) { return BadRequest(); }

                var isBought = await _studioService.IsAlbumBought(albumId);
                if (isBought == false)
                {

                    var res = await _studioService.RemoveFromPaidAlbum(trackId, albumId);
                    if (res) { return Ok(); }

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }

        }

        /// <summary>
        /// Removes an administrator from the studio.
        /// </summary>
        /// <param name="adminId">The ID of the administrator to remove.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        public async Task<IActionResult> RemoveFromAdmin(string adminId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var studioId = await _studioService.IsUserAnOwner(userId);
                if (studioId == 0) { return BadRequest(); }

                var isUserStudioAdmin = await _studioService.isUserStudioAdmin(adminId, studioId);
                if (isUserStudioAdmin == false) { return BadRequest(); }

                var removed = await _studioService.RemoveFromAdmin(adminId, studioId);
                if (removed)
                {
                    var res = _studioService.DeleteJWT(adminId);
                    return Ok();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Removes an artist from the studio.
        /// </summary>
        /// <param name="artistId">The ID of the artist to remove.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveArtistFromStudio(string artistId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var studioId = await _studioService.IsUserAnAdmin(userId);
                if (studioId == 0) { return BadRequest(); }

                var isartistinstudio = await _studioService.IsArtistInStudioByUserId(studioId, artistId);
                if (isartistinstudio == false) { return BadRequest(); }

                var res = await _studioService.DeleteArtistFromStudio(artistId, studioId);
                if (res == false) { return BadRequest(); }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }

        }

        /// <summary>
        /// Assigns a new studio head.
        /// </summary>
        /// <param name="newadminId">The ID of the new admin to promote as studio head.</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpPost]
        [Authorize(Policy = "StudioHead")]
        public async Task<IActionResult> NewStudioHead(string newadminId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var studioId = await _studioService.IsUserAnOwner(userId);
                if (studioId == 0) { return BadRequest(); }

                var isartistinstudio = await _studioService.IsArtistInStudioByUserId(studioId, newadminId);
                if (isartistinstudio == false) { return BadRequest(); }

                var head = await _studioService.BecomeHead(studioId, newadminId);
                if (head == false) { return BadRequest(); }

                var deleteOldHead = await _studioService.DeleteOldHead(userId);
                if (deleteOldHead == false) { return BadRequest(); }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }

        }

        /// <summary>
        /// Deletes the studio owned by the authenticated user.
        /// </summary>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error.</returns>
        [HttpDelete]
        [Authorize(Policy = "StudioHead")]
        public async Task<IActionResult> DeleteStudio()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var studioId = await _studioService.IsUserAnOwner(userId);
                if (studioId == 0) { return BadRequest(); }

                var removeAdmRange = await _studioService.RemoveAdminRange(studioId);
                if (removeAdmRange == false) { return BadRequest(); }

                var removeStudio = await _studioService.DeleteStudio(studioId);
                if (removeStudio == false) { return BadRequest(); }

                var removeHeadJWT = await _studioService.RemoveHead(userId);
                if (removeHeadJWT == false) { return BadRequest(); }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves albums associated with the authenticated user.
        /// </summary>
        /// <returns>A list of albums associated with the user, or StatusCode 500 on error.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAlbumByUserId()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var albums = await _studioService.GetAlbumByUserId(userId);

                return Ok(albums);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Retrieves artists associated with the studio.
        /// </summary>
        /// <returns>A list of artists associated with the studio, or StatusCode 500 on error.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetArtistByStudio()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var artists = await _studioService.GetArtistByStudio(userId);

                return Ok(artists);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
