using Image.API.Data.Common;
using Image.API.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace Image.API.Controllers
{
    [ApiController]
    [Route("image")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        // Upload an image
        // [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file,  OwnerType ownerType, Guid ownerId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");
        
            var imageUrl = await _imageService.UploadImageAsync(file, ownerType, ownerId);
            return Ok(new { Url = imageUrl });
        }

        // Update an image
        [HttpPut("update")]
        public async Task<IActionResult> UpdateImage([FromForm] IFormFile file, OwnerType ownerType,  Guid ownerId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");
        
            var imageUrl = await _imageService.UpdateImageAsync(file, ownerType, ownerId);
            return Ok(new { Url = imageUrl });
        }

        // Get the image URL
        [HttpGet("get")]
        public async Task<IActionResult> GetImageUrl([FromQuery] OwnerType ownerType, [FromQuery] Guid ownerId)
        {
            var imageUrl = await _imageService.GetImageUrlAsync(ownerType, ownerId);

            if (string.IsNullOrEmpty(imageUrl))
                return NotFound("Image not found");

            return Ok(new { Url = imageUrl });
        }
    }
}
