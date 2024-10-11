using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FileSharingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : Controller
    {
        private readonly string _fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");

        // Hiển thị trang tải lên tệp
        [HttpGet("upload")]
        public IActionResult Upload()
        {
            return View();
        }

        // Xử lý tải lên tệp
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Đường dẫn đầy đủ tới thư mục wwwroot/files
                var filePath = Path.Combine(_fileStoragePath, file.FileName);

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(_fileStoragePath))
                {
                    Directory.CreateDirectory(_fileStoragePath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return RedirectToAction("FileList");
            }

            // Trả về trang tải lên nếu không có tệp
            return View();
        }
        [HttpPost("send-file")]
        public async Task<IActionResult> SendFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine(_fileStoragePath, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { message = "File sent successfully." });
            }

            return BadRequest("File is empty.");
        }

        [HttpGet("receive-file/{fileName}")]
        public async Task<IActionResult> ReceiveFile(string fileName)
        {
            var path = Path.Combine(_fileStoragePath, fileName);

            if (!System.IO.File.Exists(path))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }



        // Hiển thị danh sách các tệp có thể tải xuống
        [HttpGet("files")]
        public IActionResult FileList()
        {
            // Lấy danh sách tệp trong thư mục
            var files = Directory.Exists(_fileStoragePath)
                ? Directory.GetFiles(_fileStoragePath).Select(Path.GetFileName).ToList()
                : new List<string>();

            return View(files);
        }

        // Xử lý tải xuống tệp
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return Content("File not found");

            var path = Path.Combine(_fileStoragePath, fileName);

            // Kiểm tra xem tệp có tồn tại không
            if (!System.IO.File.Exists(path))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        // Xác định kiểu MIME cho tệp tải xuống
        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" }
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }
}
