
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Models;
using BCrypt.Net;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Users users)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == users.Username);
            if (user == null)
            {
               return Unauthorized(new { message = "Ошибка авторизации" });
            }
            
            if (!BCrypt.Net.BCrypt.Verify(users.Password, user.Password))
            {
                return Unauthorized(new { message = "Ошибка авторизации" });
            }

            var responseUser = new LoginResponse
            {
                UserID = user.UserID,
                Username = user.Username,
                Firstname = user.Firstname,
                Middlename = user.Middlename,
                Lastname = user.Lastname,
                UserType = user.UserType
            };

            return Ok(responseUser);
        }
    }
}
