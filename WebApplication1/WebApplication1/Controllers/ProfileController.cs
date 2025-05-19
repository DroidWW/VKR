using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("profile/{UserID}")]
        public async Task<IActionResult> Profile(int UserID, [FromBody] UpdateUsers users)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == UserID);
            if (user == null)
                return Unauthorized(new { message = "Пользователь не найден" });

            user.RegistrationDate = DateTime.SpecifyKind(user.RegistrationDate, DateTimeKind.Utc);
            
            if (!string.IsNullOrEmpty(users.Username))
                user.Username = users.Username;
            
            if (!string.IsNullOrEmpty(users.Password))
                user.Password = users.Password;

            if (!string.IsNullOrEmpty(users.Firstname))
                user.Firstname = users.Firstname;

            if (!string.IsNullOrEmpty(users.Lastname))
                user.Lastname = users.Lastname;

            if (!string.IsNullOrEmpty(users.Middlename))
                user.Middlename = users.Middlename;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var image = await _context.ProfileUserImages.FirstOrDefaultAsync(u => u.UserID == UserID);
            
            if (image == null)
                return NotFound(new {message = "Нет данных"});
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile");

            if (!Directory.Exists(uploadFolder))
                return NotFound(new { message = "Нет данных" });

            var filePath = Path.Combine(uploadFolder, users.ImageURL);

            try
            {
                await System.IO.File.WriteAllBytesAsync(filePath, users.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при сохранении файла", error = ex.Message });
            }

            image.ImageURL = users.ImageURL;

            _context.ProfileUserImages.Update(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Данные успешно обновлены" });

        }

        [HttpPost("getImage")]
        public async Task<IActionResult> GetProfileImage([FromBody] int UserID)
        {

            var image = await _context.ProfileUserImages.FirstOrDefaultAsync(u => u.UserID == UserID);
            
            string imageURL;
            string imageType;

            if (image == null || image?.ImageURL == null)
            {
                imageURL = "droid_profile.png";
                imageType = "image/png";
            }
            else
            {
                imageURL = $"{image.ImageURL}";
                imageType = "image/jpeg";
            }

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile");
            
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, imageURL);

            byte[] imageData = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(imageData, imageType);
        }

    }
}
