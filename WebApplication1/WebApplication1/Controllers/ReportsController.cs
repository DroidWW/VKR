using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.Reports.OrderByDescending(u => u.ReportID).ToListAsync();

            if (reports == null)
            {
                return NotFound(new { message = "Нет записей" });
            }

            var reportsModel = new List<ReportsImages>();
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "report");

            if (!Directory.Exists(uploadFolder))
            {
                return NotFound(new { message = "Папка с изображениями не найдена" });
            }
            foreach (var report in reports)
            {
                var image = await _context.ReportImages.FirstOrDefaultAsync(u => u.ReportID == report.ReportID);

                string imageURL;

                if (image == null || image.ImageURL == null)
                {
                    imageURL = "item_icon.png";
                }
                else
                {
                    imageURL = $"{image.ImageURL}";
                }

                var filePath = Path.Combine(uploadFolder, imageURL);

                byte[] imageData = await System.IO.File.ReadAllBytesAsync(filePath);

                var likeCount = await _context.LikesDislikes
                    .Where(u => u.ReportID == report.ReportID && u.IsLiked == true)
                    .CountAsync();

                var dislikeCount = await _context.LikesDislikes
                    .Where(u => u.ReportID == report.ReportID && u.IsDisliked == true)
                    .CountAsync();

                var comments = await (
                    from ReportsComments in _context.ReportsComments
                    where ReportsComments.ReportID == report.ReportID
                    orderby ReportsComments.CommentID
                    select new ReportsComments
                    {
                        UserID = ReportsComments.UserID,
                        ReportID = ReportsComments.ReportID,
                        Text = ReportsComments.Text,
                        CreatedAt = ReportsComments.CreatedAt
                    })
                    .ToListAsync();

                reportsModel.Add(new ReportsImages
                {
                    ReportID = report.ReportID,
                    OrderID = report.OrderID,
                    MasterID = report.MasterID,
                    Name = report.Name,
                    Description = report.Description,
                    CreatedAt = report.CreatedAt,
                    Image = imageData,
                    LikeCount = likeCount,
                    DislikeCount = dislikeCount,
                    ReportsList = comments
                });

            }
            return Ok(reportsModel);
        }

        [HttpPost("checker")]
        public async Task<IActionResult> Checker([FromBody] LikesDislikes likesDislikes)
        {
            var checker = await _context.LikesDislikes
                .FirstOrDefaultAsync(u => u.ReportID == likesDislikes.ReportID && u.UserID == likesDislikes.UserID);

            if (checker == null)
            {
                var likesDislikesModel = new LikesDislikes
                {
                    ReportID = likesDislikes.ReportID,
                    UserID = likesDislikes.UserID
                };
                await _context.LikesDislikes.AddAsync(likesDislikesModel);
                await _context.SaveChangesAsync();
                return Ok(likesDislikesModel);
            }
            return Ok(checker);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] LikesDislikes likesDislikes)
        {
            var checker = await _context.LikesDislikes
                .FirstOrDefaultAsync(u => u.ReportID == likesDislikes.ReportID && u.UserID == likesDislikes.UserID);

            if (checker == null)
            {
                return NotFound(new { message = "Не найдено такого отчета" });
            }


            checker.IsLiked = likesDislikes.IsLiked;
            checker.IsDisliked = likesDislikes.IsDisliked;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Обновлено состояние" });
        }

        [HttpPost("addComment")]
        public async Task<IActionResult> AddComment([FromBody] ReportsComments reportComments)
        {
            var newComment = new ReportsComments
            {
                ReportID = reportComments.ReportID,
                UserID = reportComments.UserID,
                Text = reportComments.Text,
                CreatedAt = reportComments.CreatedAt
            };

            await _context.ReportsComments.AddAsync(newComment);
            await _context.SaveChangesAsync();

            return Ok(new {message = "Комментарий добавлен"});
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddReport([FromBody] Reports report)
        {
            var newReport = new Reports
            {
                MasterID = report.MasterID,
                Name = report.Name,
                Description = report.Description,
                CreatedAt = report.CreatedAt
            };

            await _context.Reports.AddAsync(newReport);
            await _context.SaveChangesAsync();

            return Ok(new { reportID = newReport.ReportID });
        }

        [HttpPost("images")]
        public async Task<IActionResult> UploadImage([FromBody] ImagesUploadForReports request)
        {
            if (request == null || request.Data == null || request.Data.Length == 0)
                return BadRequest("Нет данных");

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "report");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, request.Filename);

            try
            {
                await System.IO.File.WriteAllBytesAsync(filePath, request.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при сохранении файла", error = ex.Message });
            }

            var reportImage= new ReportImages
            {
                ReportID = request.ReportID,
                ImageURL = request.Filename
            };

            await _context.ReportImages.AddAsync(reportImage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Изображение успешно загрузилось" });

        }

        [HttpPut("updateReport/{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] ReportsImages report)
        {
            var reports = await _context.Reports.FindAsync(id);

            if (reports == null)
            {
                return NotFound(new { message = "Отчет не найден" });
            }

            reports.Name = report.Name;
            reports.Description = report.Description;

            await _context.SaveChangesAsync();

            if (report.Image != null)
            {
                var image = await _context.ReportImages.FirstOrDefaultAsync(u => u.ReportID == id);

                if (image == null)
                    return NotFound(new { message = "Нет данных" });

                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "report");

                if (!Directory.Exists(uploadFolder))
                    return NotFound(new { message = "Нет данных" });


                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                try
                {
                    await System.IO.File.WriteAllBytesAsync(filePath, report.Image);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Ошибка при сохранении файла", error = ex.Message });
                }
            }

            return Ok(new { message = "Отчет обновлён" });
        }

        [HttpDelete("deleteReport/{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = _context.Reports.FirstOrDefault(u => u.ReportID == id);
            if (report == null)
            {
                return NotFound(new { message = "Отчет не найден" });
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            var image = _context.ReportImages.FirstOrDefault(u => u.ReportID == id);
            if (image == null)
            {
                return NotFound(new { message = "Отчет не найден" });
            }
            _context.ReportImages.Remove(image);
            await _context.SaveChangesAsync();

            if (image.ImageURL != null)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "report");

                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                System.IO.File.Delete(filePath);
            }

            return Ok(new { message = "Отчет удален" });
        }
    }
}
