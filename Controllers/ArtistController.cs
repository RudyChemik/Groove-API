using Azure.Storage.Blobs;
using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.Services;
using Groove.VM;
using Groove.VM.Artist;
using Groove.VM.Track;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for handling artist-related operations
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ArtistController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IArtistService _artistService;
        private readonly IAccountService _accountService;
        private readonly ITrackService _trackService;
        private readonly IBlobService _blobService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="artistService">Service for managing artist-related operations</param>
        /// <param name="accountService">Service for managing user accounts</param>
        /// <param name="trackService">Service for managing tracks</param>
        /// <param name="configuration">The configuration settings</param>
        /// <param name="blobService">Service for managing blob storage</param>
        /// <param name="jwtService">Service for handling JWT authentication</param>
        public ArtistController(IArtistService artistService, IAccountService accountService, ITrackService trackService, IConfiguration configuration, IBlobService blobService, IJwtService jwtService)
        {
            _accountService = accountService;
            _artistService = artistService;
            _trackService = trackService;
            _configuration = configuration;
            _blobService = blobService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Retrieves all artists
        /// </summary>
        /// <returns>A list of all artists</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Artist>>> GetAllArtists()
        {
            try
            {
                var artists = await _artistService.GetAllArtists();
                return artists;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Allows a user to become an artist
        /// </summary>
        /// <param name="model">The model containing the information needed to become an artist</param>
        /// <returns>Returns a new JWT if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BecomeAnArtist([FromBody] BecomeArtistVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string userId = _jwtService.GetUserIdClaim();

                    var isArtist = await _artistService.IsUserArtist(userId);

                    if (isArtist)
                    {
                        return StatusCode(400, "User is already an artist");
                    }

                    var res = await _artistService.BecomeArtistAsync(model, userId);

                    if (res)
                    {
                        var jwt = await _accountService.ReturnJWT(userId);
                        return Ok(jwt);
                    }

                    return StatusCode(400, "Failed to become an artist");
                }

                return StatusCode(400, "Invalid ModelState");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new track for the artist
        /// </summary>
        /// <param name="model">The track creation model</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize(Roles = "artist")]
        public async Task<IActionResult> AddTrack([FromForm] AddTrackVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string userId = _jwtService.GetUserIdClaim();

                    var artistId = await _artistService.GetArtistId(userId);
                    string blobUrl = "";
                    if (artistId != 0)
                    {
                        if (model.Mp3File != null && model.Mp3File.Length > 0)
                        {
                            blobUrl = await _blobService.UploadBlob(model.Mp3File);
                        }

                        var res = await _artistService.AddTrack(artistId, model, blobUrl);

                        if (res)
                        {
                            return Ok();
                        }

                        return StatusCode(400, "Failed to add track");
                    }
                    else
                    {
                        return StatusCode(400, "Artist not found");
                    }
                }

                return StatusCode(400, "Invalid ModelState");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the artist's data
        /// </summary>
        /// <param name="model">The model containing the updated artist data</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize(Roles = "artist")]
        public async Task<IActionResult> UpdateArtistData([FromBody] ChnageArtistDataVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var artistId = await _artistService.GetArtistId(model.UserId);

                    if (artistId != 0)
                    {
                        var res = await _artistService.ChangeArtistData(model, artistId);

                        if (res)
                        {
                            return Ok();
                        }

                        return StatusCode(400, "Failed to update artist data");
                    }
                    else
                    {
                        return StatusCode(400, "Artist not found");
                    }
                }

                return StatusCode(400, "Invalid ModelState");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new album for the artist
        /// </summary>
        /// <param name="model">The album creation model</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize(Policy = "Artist")]
        public async Task<IActionResult> AddAlbum([FromBody] AddAlbumVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var artistId = await _artistService.GetArtistId(model.UserId);

                    if (artistId != 0)
                    {
                        var res = await _artistService.AddAlbum(artistId, model);

                        if (res)
                        {
                            return Ok();
                        }

                        return StatusCode(400, "Failed to add album");
                    }
                    else
                    {
                        return StatusCode(400, "Artist not found");
                    }
                }

                return StatusCode(400, "Invalid ModelState");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing track for the artist
        /// </summary>
        /// <param name="model">The track update model</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize(Policy = "Artist")]
        public async Task<IActionResult> UpdateTrack([FromForm] UpdateTrackVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string userId = _jwtService.GetUserIdClaim();

                    if (await _artistService.IsUserArtist(userId))
                    {
                        string blobId = await _blobService.GetBlobTrack(model.trackId);
                        await _blobService.DeleteBlob(blobId);
                        string blobURL = await _blobService.UploadBlob(model.Mp3File);

                        await _artistService.UpdateTrack(model, userId, blobURL);
                        return Ok();
                    }
                    return StatusCode(400, "User is not an artist");
                }
                return StatusCode(400, "Invalid ModelState");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a track to an album
        /// </summary>
        /// <param name="trackId">The ID of the track to add</param>
        /// <param name="albumId">The ID of the album to add the track to</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddTrackToAlbum(int trackId, int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var artistId = await _artistService.GetArtistId(userId);

                if (artistId == 0)
                {
                    return StatusCode(400, "User not an artist.");
                }

                var res = await _artistService.AddTrackToAlbum(trackId, albumId, artistId);

                if (res)
                {
                    return Ok();
                }

                return StatusCode(400, "Failed to add track to album");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies to a studio
        /// </summary>
        /// <param name="studioId">The ID of the studio to apply to</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<IActionResult> ApplyToStudio(int studioId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var artistId = await _artistService.GetArtistId(userId);
                if (artistId == 0)
                {
                    return StatusCode(400, "User does not exist");
                }

                var doesUserHaveStudio = await _artistService.DoesArtistHaveAStudio(artistId);
                if (doesUserHaveStudio)
                {
                    return StatusCode(400, "User is already assigned to the studio");
                }

                var alreadyRequested = await _artistService.AlreadyMadeRequest(artistId, studioId);
                if (alreadyRequested)
                {
                    return StatusCode(400, "Already requested");
                }

                var res = await _artistService.ApplyForStudio(artistId, studioId);
                if (res)
                {
                    return Ok();
                }
                return StatusCode(400, "Error while applying to studio");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the artist account
        /// </summary>
        /// <returns>Returns Ok with a new JWT if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpDelete]
        [Authorize(Policy = "Artist")]
        public async Task<IActionResult> DeleteArtist()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (userId != null)
                {
                    var artistId = await _artistService.GetArtistId(userId);
                    if (artistId != 0)
                    {
                        var res = await _artistService.DeleteArtist(artistId);
                        if (res)
                        {
                            return Ok(await _accountService.ReturnJWT(userId));
                        }

                        return StatusCode(400, "Failed to delete artist");
                    }
                    return StatusCode(400, "Artist not found");
                }

                return StatusCode(400, "Invalid UserId");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a track
        /// </summary>
        /// <param name="trackId">The ID of the track to delete</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpDelete]
        [Authorize(Policy = "Artist")]
        public async Task<IActionResult> DeleteTrack(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (userId != null)
                {
                    var res = await _artistService.DeleteTrack(userId, trackId);
                    if (res)
                    {
                        return Ok();
                    }
                    return StatusCode(400, "Failed to delete track");
                }

                return StatusCode(400, "Invalid UserId");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an album
        /// </summary>
        /// <param name="albumId">The ID of the album to delete</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpDelete]
        [Authorize(Policy = "Artist")]
        public async Task<IActionResult> DeleteAlbum(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (userId != null)
                {
                    var res = await _artistService.DeleteAlbum(userId, albumId);
                    if (res)
                    {
                        return Ok();
                    }
                    return StatusCode(400, "Failed to delete album");
                }

                return StatusCode(400, "Invalid UserId");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a track from an album
        /// </summary>
        /// <param name="trackId">The ID of the track to remove</param>
        /// <param name="albumId">The ID of the album</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveFromAlbum(int trackId, int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var artistId = await _artistService.GetArtistId(userId);

                if (artistId == 0)
                {
                    return StatusCode(400, "Artist not found");
                }

                var res = await _artistService.RemoveFromAlbum(trackId, albumId, artistId);

                if (res)
                {
                    return Ok();
                }

                return StatusCode(400, "Failed to remove track from album");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates an album with tracks for the artist
        /// </summary>
        /// <param name="model">The model containing album and track information</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAlbumWithTracks([FromForm] CreateAlbumWithTracksVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var artistId = await _artistService.GetArtistId(userId);

                if (artistId == 0)
                {
                    return StatusCode(400, "Artist not found");
                }

                var res = await _artistService.CreateAlbumWithTracks(model, artistId);
                if (res)
                {
                    return Ok();
                }

                return StatusCode(400, "Failed to create album with tracks");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves albums associated with the authenticated user
        /// </summary>
        /// <returns>A list of albums associated with the user, or StatusCode 500 on error</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAlbumByUserId()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var albums = await _artistService.GetAlbumByUserId(userId);

                return Ok(albums);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
