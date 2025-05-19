using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Data.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminAuthController: ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public AdminAuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("auth")]
        public async Task<IActionResult> AdminAuth([FromBody] Users users)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == users.Username && u.UserType == 3);

            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Нет доступа" });
            }

            if (!BCrypt.Net.BCrypt.Verify(users.Password, user.Password))
            {
                return Unauthorized(new { success = false, message = "Нет доступа" });
            }

            HttpContext.Session.SetInt32("UserId", user.UserID);
            return Ok(new { success = true, message = "Есть доступ" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true });
        }

        [HttpGet("isAuthed")]
        public async Task<IActionResult> IsAdminAuthed()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            return Ok(new { success = true, message = "Есть доступ" });
        }

        //МАСТЕР
        [HttpPost("addMaster")]
        public async Task<IActionResult> AddMaster([FromBody] Users users)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == users.Username);

            if (user != null)
            {
                return Ok(new {success = false, message = "Пользователь с таким именем уже существует" });
            }

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(users.Password);

            var newUser = new Users
            {
                Username = users.Username,
                Password = hashPassword,
                Firstname = users.Firstname,
                Lastname = users.Lastname,
                Middlename = users.Middlename,
                UserType = 2,
                RegistrationDate = DateTime.UtcNow.Date
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Пользователь успешно добавлен" });
        }

        [HttpGet("getMasters")]
        public async Task<IActionResult> GetAllMasters()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var users = await _context.Users.Where(u => u.UserType == 2).OrderByDescending(u => u.UserID).ToListAsync();

            if (users == null)
            {
                return NotFound(new { success = true, message = "Нет записей" });
            }

            return Ok(new { success = true, data = users });
        }

        [HttpDelete("deleteMaster/{id}")]
        public async Task<IActionResult> DeleteMaster(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = _context.Users.FirstOrDefault(u => u.UserID == id);
            if(user == null)
            {
                return NotFound(new { success = false, message = "Пользователь не найден" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var image = _context.ProfileUserImages.FirstOrDefault(u => u.UserID == id);
            if (image == null)
            {
                return NotFound(new { success = false, message = "Пользователь не найден" });
            }
            _context.ProfileUserImages.Remove(image);
            await _context.SaveChangesAsync();

            if (image.ImageURL != null)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile");

                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                System.IO.File.Delete(filePath);
            }

            return Ok(new { success = true, message = "Пользователь удален" });
        }

        [HttpGet("getMaster/{id}")]
        public async Task<IActionResult> GetMasterById(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.UserType != 2)
            {
                return NotFound(new { success = false, message = "Мастер не найден" });
            }

            return Ok(new { success = true, data = user });
        }

        [HttpPut("updateMaster/{id}")]
        public async Task<IActionResult> UpdateMaster(int id, [FromBody] Users updatedUser)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.UserType != 2)
            {
                return NotFound(new { success = false, message = "Мастер не найден" });
            }

            user.Username = updatedUser.Username;
            user.Password = updatedUser.Password;
            user.Firstname = updatedUser.Firstname;
            user.Middlename = updatedUser.Middlename;
            user.Lastname = updatedUser.Lastname;
            user.Password = updatedUser.Password;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Мастер обновлён" });
        }

        //ПОЛЬЗОВАТЕЛЬ
        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser([FromBody] Users users)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == users.Username);

            if (user != null)
            {
                return Ok(new { success = false, message = "Пользователь с таким именем уже существует" });
            }

            var newUser = new Users
            {
                Username = users.Username,
                Password = users.Password,
                Firstname = users.Firstname,
                Lastname = users.Lastname,
                Middlename = users.Middlename,
                UserType = 1,
                RegistrationDate = DateTime.UtcNow.Date
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Пользователь успешно добавлен" });
        }

        [HttpGet("getUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var users = await _context.Users.Where(u => u.UserType == 1).OrderByDescending(u => u.UserID).ToListAsync();

            if (users == null)
            {
                return NotFound(new { success = true, message = "Нет записей" });
            }

            return Ok(new { success = true, data = users });
        }

        [HttpDelete("deleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = _context.Users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Пользователь не найден" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var image = _context.ProfileUserImages.FirstOrDefault(u => u.UserID == id);
            if (image == null)
            {
                return NotFound(new { success = false, message = "Пользователь не найден" });
            }
            _context.ProfileUserImages.Remove(image);
            await _context.SaveChangesAsync();

            if (image.ImageURL != null)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile");

                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                System.IO.File.Delete(filePath);
            }

            return Ok(new { success = true, message = "Пользователь удален" });
        }

        [HttpGet("getUser/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.UserType != 1)
            {
                return NotFound(new { success = false, message = "Пользователь не найден" });
            }

            return Ok(new { success = true, data = user });
        }

        [HttpPut("updateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] Users updatedUser)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.UserType != 1)
            {
                return NotFound(new { success = false, message = "Пользователь не найден" });
            }

            user.Username = updatedUser.Username;
            user.Password = updatedUser.Password;
            user.Firstname = updatedUser.Firstname;
            user.Middlename = updatedUser.Middlename;
            user.Lastname = updatedUser.Lastname;
            user.Password = updatedUser.Password;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Пользователь обновлён" });
        }

        //ОТЧЕТ
        [HttpPost("addReport")]
        public async Task<IActionResult> AddReport([FromBody] Reports report)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var newReport = new Reports
            {
                Name = report.Name,
                Description = report.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Reports.AddAsync(newReport);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Отчет успешно добавлен" });
        }

        [HttpGet("getReports")]
        public async Task<IActionResult> GetAllReports()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var users = await _context.Reports.OrderByDescending(u => u.ReportID).ToListAsync();

            if (users == null)
            {
                return NotFound(new { success = true, message = "Нет записей" });
            }

            return Ok(new { success = true, data = users });
        }

        [HttpDelete("deleteReport/{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var report = _context.Reports.FirstOrDefault(u => u.ReportID == id);
            if (report == null)
            {
                return NotFound(new { success = false, message = "Отчет не найден" });
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            var image = _context.ReportImages.FirstOrDefault(u => u.ReportID == id);
            if (image == null)
            {
                return NotFound(new { success = false, message = "Отчет не найден" });
            }
            _context.ReportImages.Remove(image);
            await _context.SaveChangesAsync();

            if (image.ImageURL != null)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "report");

                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                System.IO.File.Delete(filePath);
            }

            return Ok(new { success = true, message = "Отчет удален" });
        }

        [HttpGet("getReport/{id}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = await _context.Reports.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Отчет не найден" });
            }

            return Ok(new { success = true, data = user });
        }

        [HttpPut("updateReport/{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] Reports report)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var reports = await _context.Reports.FindAsync(id);

            if (reports == null)
            {
                return NotFound(new { success = false, message = "Отчет не найден" });
            }

            reports.Name = report.Name;
            reports.Description = report.Description;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Отчет обновлён" });
        }

        //МАГАЗИН
        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct([FromBody] Products product)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var newProduct = new Products
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Товар успешно добавлен" });
        }

        [HttpGet("getProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var users = await _context.Products.OrderByDescending(u => u.ProductID).ToListAsync();

            if (users == null)
            {
                return NotFound(new { success = true, message = "Нет записей" });
            }

            return Ok(new { success = true, data = users });
        }

        [HttpDelete("deleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var product = _context.Products.FirstOrDefault(u => u.ProductID == id);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Товар не найден" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            var image = _context.ProductImages.FirstOrDefault(u => u.ProductID == id);
            if (image == null)
            {
                return NotFound(new { success = false, message = "Отчет не найден" });
            }
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            if (image.ImageURL != null)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");

                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                System.IO.File.Delete(filePath);
            }

            return Ok(new { success = true, message = "Товар удален" });
        }

        [HttpGet("getProduct/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var user = await _context.Products.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Товар не найден" });
            }

            return Ok(new { success = true, data = user });
        }

        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Products product)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Ай-ай-ай шалунишка" });
            }

            var products = await _context.Products.FindAsync(id);

            if (products == null)
            {
                return NotFound(new { success = false, message = "Товар не найден" });
            }

            products.Name = product.Name;
            products.Description = product.Description;
            products.Price = product.Price;
            
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Товар обновлён" });
        }

        //ЗАКАЗЫ
    }
}
