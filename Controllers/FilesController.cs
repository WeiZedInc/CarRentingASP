using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadsFolder;

        public FilesController(ILogger<FilesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _uploadsFolder = _configuration["FileStorage:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        // POST: api/files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string folderName = "documents")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            try
            {
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest("File size exceeds the limit (10MB).");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };

                if (!Array.Exists(allowedExtensions, ext => ext == extension))
                {
                    return BadRequest("File type not allowed. Allowed types: jpg, jpeg, png, pdf, doc, docx");
                }

                string targetFolder = Path.Combine(_uploadsFolder, folderName);
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(targetFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string fileUrl = $"{Request.Scheme}://{Request.Host}/api/files/download/{folderName}/{uniqueFileName}";

                _logger.LogInformation($"File uploaded successfully: {fileUrl}");

                return Ok(new { fileName = uniqueFileName, originalFileName = file.FileName, fileUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, "An error occurred while uploading the file.");
            }
        }

        // GET: api/files/download/documents/filename.jpg
        [HttpGet("download/{folderName}/{fileName}")]
        [AllowAnonymous] // Allow public access to downloaded files
        public IActionResult DownloadFile(string folderName, string fileName)
        {
            try
            {
                if (folderName.Contains("..") || folderName.Contains("/") || folderName.Contains("\\"))
                {
                    return BadRequest("Invalid folder name.");
                }

                if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return BadRequest("Invalid file name.");
                }

                string filePath = Path.Combine(_uploadsFolder, folderName, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".pdf" => "application/pdf",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                return File(fileStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }
    }
}