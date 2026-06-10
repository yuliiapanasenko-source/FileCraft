using Microsoft.AspNetCore.Mvc;

using FileSharingSystem;

namespace FileSharingSystem.Api.Controllers
{
    // DTO для запитів
    public record UploadRequest(int FileId, int UserId, string FileName, string FileData);
    public record TogglePublicRequest(bool IsPublic);
    public record AddTagRequest(int TagId, string Label);
    public record AnalyzeRequest(string Content);

    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        // Тимчасове in-memory сховище (заміниш на БД пізніше)
        private static readonly Dictionary<int, FileItem> _store = new();
        private const string ShareBaseUrl = "https://filecraft.up.railway.app/files/";

        // ──────────────────────────────────────────────────────────────
        // GET /api/files
        // Повертає список усіх файлів у сховищі
        // ──────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetAll()
        {
            var files = _store.Values.Select(f => new
            {
                f.FileId,
                f.UserId,
                f.FileName,
                f.IsPublic,
                f.ShareUrl,
                TagCount = f.Tags.Count,
                ReportCount = f.Reports.Count
            });

            return Ok(new { total = _store.Count, files });
        }

        // ──────────────────────────────────────────────────────────────
        // GET /api/files/{id}
        // Повертає один файл за FileId
        // ──────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!_store.TryGetValue(id, out var file))
                return NotFound(new { error = $"Файл з id={id} не знайдено" });

            return Ok(new
            {
                file.FileId,
                file.UserId,
                file.FileName,
                file.IsPublic,
                file.ShareUrl,
                file.Tags,
                file.Reports
            });
        }

        // ──────────────────────────────────────────────────────────────
        // POST /api/files/upload
        // Завантажує файл — викликає FileItem.Upload()
        //
        // Body: { "fileId": 1, "userId": 10, "fileName": "doc.pdf", "fileData": "..." }
        // ──────────────────────────────────────────────────────────────
        [HttpPost("upload")]
        public IActionResult Upload([FromBody] UploadRequest req)
        {
            try
            {
                var file = new FileItem(ShareBaseUrl, req.FileName, string.Empty)
                {
                    FileId = req.FileId,
                    UserId = req.UserId,
                    FileName = req.FileName
                };

                file.Upload(req.FileData);
                _store[file.FileId] = file;

                return Ok(new
                {
                    message = "Файл успішно завантажено",
                    fileId = file.FileId,
                    report = file.Reports.LastOrDefault()?.Content
                });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────────
        // PATCH /api/files/{id}/visibility
        // Змінює публічність файлу — викликає FileItem.TogglePublic()
        //
        // Body: { "isPublic": true }
        // ──────────────────────────────────────────────────────────────
        [HttpPatch("{id:int}/visibility")]
        public IActionResult ToggleVisibility(int id, [FromBody] TogglePublicRequest req)
        {
            if (!_store.TryGetValue(id, out var file))
                return NotFound(new { error = $"Файл з id={id} не знайдено" });

            try
            {
                string shareUrl = file.TogglePublic(req.IsPublic);

                return Ok(new
                {
                    message = req.IsPublic ? "Файл відкрито для публічного доступу" : "Файл закрито",
                    isPublic = file.IsPublic,
                    shareUrl = string.IsNullOrEmpty(shareUrl) ? null : shareUrl
                });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────────
        // POST /api/files/{id}/tags
        // Додає тег до файлу — викликає FileItem.AddTag()
        //
        // Body: { "tagId": 1, "label": "Навчання" }
        // ──────────────────────────────────────────────────────────────
        [HttpPost("{id:int}/tags")]
        public IActionResult AddTag(int id, [FromBody] AddTagRequest req)
        {
            if (!_store.TryGetValue(id, out var file))
                return NotFound(new { error = $"Файл з id={id} не знайдено" });

            try
            {
                var tag = new Tag { TagId = req.TagId, Label = req.Label };
                file.AddTag(tag);

                return Ok(new
                {
                    message = "Тег додано",
                    tagCount = file.Tags.Count,
                    tags = file.Tags
                });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────────
        // GET /api/files/search?tag=Навчання
        // Пошук файлів за міткою тегу
        // ──────────────────────────────────────────────────────────────
        [HttpGet("search")]
        public IActionResult SearchByTag([FromQuery] string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return BadRequest(new { error = "Параметр 'tag' не може бути порожнім" });

            var results = _store.Values
                .Where(f => f.Tags.Any(t =>
                    t.Label.Contains(tag, StringComparison.OrdinalIgnoreCase)))
                .Select(f => new
                {
                    f.FileId,
                    f.FileName,
                    f.IsPublic,
                    matchedTags = f.Tags
                        .Where(t => t.Label.Contains(tag, StringComparison.OrdinalIgnoreCase))
                        .Select(t => t.Label)
                });

            return Ok(new { query = tag, results });
        }

        // ──────────────────────────────────────────────────────────────
        // POST /api/files/analyze
        // AI-аналіз тексту (порт логіки з analyzeFile.js / MainCode.js)
        //
        // Body: { "content": "текст для аналізу" }
        // ──────────────────────────────────────────────────────────────
        [HttpPost("analyze")]
        public IActionResult Analyze([FromBody] AnalyzeRequest req)
        {
            if (string.IsNullOrEmpty(req.Content))
                return BadRequest(new { error = "No content provided" });

            var summary = req.Content.Length > 100
                ? req.Content[..100]
                : req.Content;

            var wordCount = req.Content
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Length;

            return Ok(new
            {
                message = "File processed successfully",
                data = new { summary, wordCount }
            });
        }

        // ──────────────────────────────────────────────────────────────
        // DELETE /api/files/{id}
        // Видаляє файл зі сховища
        // ──────────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!_store.ContainsKey(id))
                return NotFound(new { error = $"Файл з id={id} не знайдено" });

            _store.Remove(id);
            return Ok(new { message = $"Файл id={id} успішно видалено" });
        }
    }
}
