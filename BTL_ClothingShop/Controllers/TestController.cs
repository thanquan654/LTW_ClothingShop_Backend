using BTL_ClothingShop.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BTL_ClothingShop.Controllers
{

    public class UploadImageRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }

        // Nhiều ảnh
        public List<IFormFile> Files { get; set; }
    }


    [ApiController]
    [Route("/api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpPost("images")]
        public async Task<IActionResult> UploadImages([FromForm] UploadImageRequest request)
        {
            // Print request to console
            Console.WriteLine($"Request: {request}");

            if (request.Files == null || request.Files.Count == 0)
                return BadRequest("Không có file nào được gửi");

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var imageUrls = new List<string>();

            foreach (var file in request.Files)
            {
                var fileExt = Path.GetExtension(file.FileName);
                var fileName = Guid.NewGuid() + fileExt;
                var filePath = Path.Combine(uploadFolder, fileName);

                // Print file path to console
                Console.WriteLine($"File path: {filePath}");

                using (var stream = System.IO.File.Create(filePath)) {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

                imageUrls.Add(imageUrl);
            }

            return Ok(new
            {
                request.Title,
                request.Description,
                Images = imageUrls
            });
        }

    }
}
