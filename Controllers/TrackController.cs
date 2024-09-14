using Groove.Interfaces;
using Groove.Models;
using Groove.VMO.Track;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for handling operations related to tracks, including fetching tracks by various filters 
    /// such as artist, album, and studio, as well as handling paid tracks.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TrackController : Controller
    {
        private readonly ITrackService _trackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackController"/> class.
        /// </summary>
        /// <param name="trackService">Service for track operations.</param>
        public TrackController(ITrackService trackService)
        {
            _trackService = trackService;
        }

        /// <summary>
        /// Retrieves all available tracks.
        /// </summary>
        /// <returns>A list of all tracks.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAllTracksVM>>> GetAllTracks()
        {
            try
            {
                var tracks = await _trackService.GetAllTracks();
                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific track by its ID.
        /// </summary>
        /// <param name="id">The ID of the track to retrieve.</param>
        /// <returns>The track with the specified ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Track>> GetTrackById(int id)
        {
            try
            {
                var track = await _trackService.GetTrackById(id);

                if (track == null)
                {
                    return NotFound($"{id} not found");
                }

                return Ok(track);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all tracks by a specific artist.
        /// </summary>
        /// <param name="artistId">The ID of the artist.</param>
        /// <returns>A list of tracks by the specified artist, or NotFound if none are found.</returns>
        [HttpGet("{artistId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Track>>> GetTrackByArtistId(int artistId)
        {
            try
            {
                var tracks = await _trackService.GetTrackByArtistId(artistId);

                if (tracks == null || tracks.Count == 0)
                {
                    return NotFound($"No tracks found for artist {artistId}");
                }

                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all tracks from a specific album.
        /// </summary>
        /// <param name="albumId">The ID of the album.</param>
        /// <returns>A list of tracks from the specified album, or NotFound if none are found.</returns>
        [HttpGet("{albumId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Track>>> GetTrackByAlbumId(int albumId)
        {
            try
            {
                var tracks = await _trackService.GetTrackByAlbumId(albumId);

                if (tracks == null || tracks.Count == 0)
                {
                    return NotFound($"No tracks found for album {albumId}");
                }

                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all paid tracks.
        /// </summary>
        /// <returns>A list of all paid tracks, or NotFound if none are found.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaidTracks>>> GetAllPaidTracks()
        {
            try
            {
                var tracks = await _trackService.GetPaidTracks();

                if (tracks == null || tracks.Count == 0)
                {
                    return NotFound("No paid tracks found");
                }

                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all paid tracks by a specific artist.
        /// </summary>
        /// <param name="artistId">The ID of the artist.</param>
        /// <returns>A list of paid tracks by the specified artist, or NotFound if none are found.</returns>
        [HttpGet("{artistId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PaidTracks>>> GetPaidTrackByArtistId(int artistId)
        {
            try
            {
                var tracks = await _trackService.GetPaidTrackByArtistId(artistId);

                if (tracks == null || tracks.Count == 0)
                {
                    return NotFound($"No paid tracks found for artist {artistId}.");
                }

                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all paid tracks by a specific studio.
        /// </summary>
        /// <param name="studioId">The ID of the studio.</param>
        /// <returns>A list of paid tracks by the specified studio, or NotFound if none are found.</returns>
        [HttpGet("{studioId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PaidTracks>>> GetPaidTrackByStudioId(int studioId)
        {
            try
            {
                var tracks = await _trackService.GetPaidTrackByStudio(studioId);

                if (tracks == null || tracks.Count == 0)
                {
                    return NotFound($"No paid tracks found for studio {studioId}.");
                }

                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all tracks by a specific studio.
        /// </summary>
        /// <param name="studioId">The ID of the studio.</param>
        /// <returns>A list of tracks by the specified studio, or NotFound if none are found.</returns>
        [HttpGet("{studioId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Track>>> GetTrackByStudioId(int studioId)
        {
            try
            {
                var tracks = await _trackService.GetTrackByStudioId(studioId);

                if (tracks == null || tracks.Count == 0)
                {
                    return NotFound($"No tracks found for studio {studioId}.");
                }

                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific paid track by its ID.
        /// </summary>
        /// <param name="id">The ID of the paid track to retrieve.</param>
        /// <returns>The paid track with the specified ID, or NotFound if not found.</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<PaidTracks>> GetPaidTrackById(int id)
        {
            try
            {
                var track = await _trackService.GetPaidTrackById(id);

                if (track == null)
                {
                    return NotFound($"Track with ID {id} not found.");
                }

                return Ok(track);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }
    }
}
