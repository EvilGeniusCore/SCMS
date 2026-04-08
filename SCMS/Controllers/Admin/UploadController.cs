using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SCMS.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    [Route("admin/upload")]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UploadController> _logger;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".ico"
        };

        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml", "image/bmp", "image/x-icon"
        };

        public UploadController(IWebHostEnvironment env, ILogger<UploadController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Upload rejected: invalid extension '{Extension}' for file '{FileName}'", extension, file.FileName);
                return BadRequest($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
            }

            if (!AllowedMimeTypes.Contains(file.ContentType))
            {
                _logger.LogWarning("Upload rejected: invalid MIME type '{ContentType}' for file '{FileName}'", file.ContentType, file.FileName);
                return BadRequest($"MIME type '{file.ContentType}' is not allowed.");
            }

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "temp");
            Directory.CreateDirectory(uploadsPath);

            // Generate a unique filename to prevent collisions and path traversal
            var uniqueName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Upload complete: {FileName} -> /uploads/temp/{UniqueName}", file.FileName, uniqueName);

            return Json(new { location = $"/uploads/temp/{uniqueName}" });
        }
    }
}
