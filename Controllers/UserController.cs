using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.Services;
using Groove.VMO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for handling user-related operations such as fetching user liked tracks and albums
    /// managing user track and album prefs, and handling user download requests
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITrackService _trackService;
        private readonly IAlbumService _albumService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="userService">Service for user operations</param>
        /// <param name="trackService">Service for track operations</param>
        /// <param name="albumService">Service for album operations</param>
        /// <param name="jwtService">Service for handling JWT authentication</param>
        public UserController(IUserService userService, ITrackService trackService, IAlbumService albumService, IJwtService jwtService)
        {
            _userService = userService;
            _trackService = trackService;
            _albumService = albumService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Retrieves all tracks liked by the authenticated user
        /// </summary>
        /// <returns>A list of tracks liked by the user</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Track>>> GetAllUserLikedTracks()
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var userIdClaim = identity.FindFirst("UserId");

                    if (userIdClaim != null)
                    {
                        string userId = userIdClaim.Value;

                        try
                        {
                            var likedTracks = await _userService.GetAllUserLikedTracks(userId);
                            return Ok(likedTracks);
                        }
                        catch (Exception ex)
                        {
                            return NotFound(ex.Message);
                        }
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all albums liked by the authenticated user
        /// </summary>
        /// <returns>A list of albums liked by the user</returns>
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Album>>> GetAllUserLikedAlbums()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                try
                {
                    var likedAlbums = await _userService.GetAllUserLikedAlbums(userId);
                    return Ok(likedAlbums);
                }
                catch (Exception ex)
                {
                    return NotFound(ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Marks a track as liked by the authenticated user
        /// </summary>
        /// <param name="trackId">The ID of the track to like0< /param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost("{trackId}")]
        [Authorize]
        public async Task<IActionResult> LikeTrack(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var success = await _userService.LikeTrack(userId, trackId);
                if (success)
                    return Ok();

                    return BadRequest("DB error while adding");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Marks an album as liked by the authenticated user
        /// </summary>
        /// <param name="albumId">The ID of the album to like</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost("{albumId}")]
        [Authorize]
        public async Task<IActionResult> LikeAlbum(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var success = await _userService.LikeAlbum(userId, albumId);
                if (success)
                    return Ok();

                    return BadRequest("DB error while adding");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a track from the authenticated user's liked list
        /// </summary>
        /// <param name="trackId">The ID of the track to dislike</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost("{trackId}")]
        [Authorize]
        public async Task<IActionResult> DislikeTrack(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var success = await _userService.DislikeTrack(userId, trackId);

                if (success)
                    return Ok();

                return BadRequest("DB error while removing");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes an album from the authenticated user's liked list
        /// </summary>
        /// <param name="albumId">The ID of the album to dislike</param>
        /// <returns>Returns Ok if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost("{albumId}")]
        [Authorize]
        public async Task<IActionResult> DislikeAlbum(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var success = await _userService.DislikeAlbum(userId, albumId);

                if (success)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("DB error while removing");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves information about the authenticated user
        /// </summary>
        /// <returns>The user information, or BadRequest if not found, or StatusCode 500 on error</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                try
                {
                    var res = await _userService.GetUserWithInformation(userId);

                    if (res == null)
                    {
                        return BadRequest("User does not have an information instance");
                    }

                    return Ok(res);

                }
                catch (Exception ex)
                {
                    return BadRequest($"error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the order history of the authenticated user
        /// </summary>
        /// <returns>A list of user orders, or StatusCode 500 on error</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderHistory>>> GetUserOrders()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                return await _userService.GetUserOrders(userId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific order by its ID for the authenticated user
        /// </summary>
        /// <param name="id">The ID of the order to retrieve</param>
        /// <returns>The order details, or BadRequest if not found, or StatusCode 500 on error</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<OrderHistory>> GetUserOrderById(int id)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _userService.GetOrderHistoryById(id, userId);
                if (res == null) { return BadRequest("null"); }

                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Allows the authenticated user to download tracks from an ordered album
        /// </summary>
        /// <param name="orderId">The ID of the order containing the tracks to download.</param>
        /// <returns>A list of downloadable tracks, or BadRequest if not found, or StatusCode 500 on error</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<DownloadVM>>> DownloadOrdered(int orderId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _userService.GetOrderHistoryById(orderId, userId);
                if (res == null) { return BadRequest("null"); }

                var listDown = new List<DownloadVM>();

                foreach (var purchasedTrack in res.PurchasedItems)
                {
                    if (purchasedTrack != null)
                    {
                        var download = new DownloadVM
                        {
                            Name = purchasedTrack.PaidTrack.Name,
                            BlobUrl = purchasedTrack.PaidTrack.BlobUrl
                        };
                        listDown.Add(download);
                    }
                }

                foreach (var purchasedAlbum in res.PurchasedAlbums)
                {
                    if (purchasedAlbum.PaidAlbum != null)
                    {
                        foreach (var albumTrack in purchasedAlbum.PaidAlbum.Tracks)
                        {
                            var download = new DownloadVM
                            {
                                Name = albumTrack.Name,
                                BlobUrl = albumTrack.BlobUrl
                            };
                            listDown.Add(download);
                        }
                    }
                }

                return listDown;

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Allows the authenticated user to download a specific purchased track
        /// </summary>
        /// <param name="trackId">The ID of the track to download</param>
        /// <returns>The downloadable track details, or BadRequest if not purchased, or StatusCode 500 on error</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<DownloadVM>> DownloadPurchasedTrack(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var hasPurchasedTrack = await _userService.HasUserPurchasedTrack(userId, trackId);

                if (!hasPurchasedTrack)
                {
                    return BadRequest();
                }

                var track = await _trackService.GetPaidTrackById(trackId);

                if (track == null)
                {
                    return NotFound();
                }

                var download = new DownloadVM
                {
                    Name = track.Name,
                    BlobUrl = track.BlobUrl
                };

                return download;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all tracks purchased by the authenticated user
        /// </summary>
        /// <returns>A list of purchased tracks, or StatusCode 500 on error</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<PaidTracks>>> GetPurchasedTracksByUser()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var purchasedTracks = await _userService.GetPurchasedTracksByUser(userId);

                return Ok(purchasedTracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all albums purchased by the authenticated user
        /// </summary>
        /// <returns>A list of purchased albums, or StatusCode 500 on error</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<PaidTracks>>> GetPurchasedAlbumsByUser()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var ablums = await _albumService.GetPurchasedAlbumsByUser(userId);

                return Ok(ablums);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Allows the authenticated user to download all tracks from a purchased album
        /// </summary>
        /// <param name="albumId">The ID of the album to download tracks from</param>
        /// <returns>A list of downloadable tracks, or BadRequest if not purchased, or StatusCode 500 on error</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<DownloadVM>>> DownloadPurchasedAlbumTracks(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var hasPurchasedAlbum = await _albumService.HasUserPurchasedAlbum(userId, albumId);

                if (!hasPurchasedAlbum)
                {
                    return BadRequest();
                }

                var albumTracks = await _albumService.GetPurchasedAlbumTracks(albumId);

                if (albumTracks == null || albumTracks.Count == 0)
                {
                    return NotFound();
                }

                var downloadList = new List<DownloadVM>();

                foreach (var track in albumTracks)
                {
                    var download = new DownloadVM
                    {
                        Name = track.Name,
                        BlobUrl = track.BlobUrl
                    };
                    downloadList.Add(download);
                }

                return Ok(downloadList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the balance of the authenticated user
        /// </summary>
        /// <returns>The user's balance, or BadRequest if user not found, or StatusCode 500 on error</returns>
        [HttpGet]
        public async Task<ActionResult<double>> GetUserBalance()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest("NotFound");

                var balance = await _userService.GetUserBalance(userId);

                return Ok(balance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

    }
}
