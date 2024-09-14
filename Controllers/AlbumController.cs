using Groove.Interfaces;
using Groove.Models;
using Groove.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for handling album-related operations
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AlbumController : Controller
    {
        private readonly IAlbumService _albumService;

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="albumService">Service for managing album-related operations</param>
        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        /// <summary>
        /// Retrieves all albums
        /// </summary>
        /// <returns>A list of all albums</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Album>>> GetAllAlbums()
        {
            try
            {
                var albums = await _albumService.GetAllAlbums();
                return albums;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves an album by its ID
        /// </summary>
        /// <param name="id">The ID of the album to retrieve</param>
        /// <returns>The album with the specified ID, or NotFound if not found</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Album>> GetAlbymById(int id)
        {
            try
            {
                var album = await _albumService.GetAlbumById(id);

                if (album == null)
                {
                    return NotFound();
                }

                return album;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all paid albums
        /// </summary>
        /// <returns>A list of all paid albums</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PaidAlbums>>> GetAllPaidAlbums()
        {
            try
            {
                var albums = await _albumService.GetAllPaid();
                if (albums == null)
                {
                    return BadRequest();
                }

                return albums;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves albums by a specific artist ID
        /// </summary>
        /// <param name="artistId">The ID of the artist</param>
        /// <returns>A list of albums by the specified artist, or NotFound if none are found</returns>
        [HttpGet("{artistId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Album>>> GetAlbumByArtistId(int artistId)
        {
            try
            {
                var albums = await _albumService.GetAlbumByArtistId(artistId);

                if (albums == null || albums.Count == 0)
                {
                    return NotFound($"No albums found for artist {artistId}.");
                }

                return Ok(albums);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Retrieves albums by a specific studio ID
        /// </summary>
        /// <param name="studioId">The ID of the studio</param>
        /// <returns>A list of albums by the specified studio, or NotFound if none are found</returns>
        [HttpGet("{studioId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Album>>> GetAlbumByStudioId(int studioId)
        {
            try
            {
                var albums = await _albumService.GetAlbumByStudioId(studioId);

                if (albums == null || albums.Count == 0)
                {
                    return NotFound($"No albums found for studio {studioId}.");
                }

                return Ok(albums);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
