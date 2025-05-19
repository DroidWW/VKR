using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Registration([FromBody] Users users)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == users.Username);

            if (user != null) 
            {
                return BadRequest(new { message = "Регистрация невозможна, такой пользователь уже существует" });
            }

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(users.Password);

            var newUser = new Users
            {
                Username = users.Username,
                Password = hashPassword,
                Firstname = users.Firstname,
                Lastname = users.Lastname,
                Middlename = users.Middlename,
                UserType = users.UserType,
                RegistrationDate = DateTime.UtcNow.Date
            };

            
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var newUserImage = new ProfileUserImages
            {
                UserID = newUser.UserID
            };

            await _context.ProfileUserImages.AddAsync(newUserImage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна" });
        }

    }
}
