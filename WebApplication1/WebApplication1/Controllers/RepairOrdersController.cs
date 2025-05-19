using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepairOrdersController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RepairOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddRepairOrder([FromBody] RepairOrders orders)
        {

            var newOrder = new RepairOrders
            {
                ClientID = orders.ClientID,
                Name = orders.Name,
                Description = orders.Description,
                CreatedAt = DateTime.UtcNow,
                StatusID = orders.StatusID,
                ServicesID = orders.ServicesID
            };

            await _context.RepairOrders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new { orderID = newOrder.OrderID });
        }

        [HttpPost("images")]
        public async Task<IActionResult> UploadImage([FromBody] ImagesUploadForOrders request)
        {
            if (request == null || request.Data == null || request.Data.Length == 0)
                return BadRequest("Нет данных");

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "order");

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

            var repairOrdersImage = new RepairOrderImages
            {
                OrderID = request.OrderID,
                ImageURL = request.Filename
            };

            await _context.RepairOrderImages.AddAsync(repairOrdersImage);
            await _context.SaveChangesAsync();

            return Ok(new {message = "Изображение успешно загрузилось"});

        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.RepairOrders.Where(u => u.StatusID == 0).OrderByDescending(u => u.OrderID).ToListAsync();

            if (orders == null)
            {
                return NotFound(new { message = "Нет записей" });
            }

            var ordersModel = new List<UpdateRepairOrders>();
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "order");

            if (!Directory.Exists(uploadFolder))
            {
                return NotFound(new { message = "Папка с изображениями не найдена" });
            }

            foreach (var order in orders)
            {
                var image = await _context.RepairOrderImages.FirstOrDefaultAsync(u => u.OrderID == order.OrderID);

                string imageURL;

                if (image == null)
                {
                    imageURL = "item_icon.png";
                }
                else
                {
                    imageURL = $"{image.ImageURL}";
                }

                var filePath = Path.Combine(uploadFolder, imageURL);

                byte[] imageData = await System.IO.File.ReadAllBytesAsync(filePath);


                ordersModel.Add(new UpdateRepairOrders
                {
                    OrderID = order.OrderID,
                    ClientID = order.ClientID,
                    MasterID = order.MasterID,
                    ServicesID = order.ServicesID,
                    Name = order.Name,
                    Description = order.Description,
                    StatusID = order.StatusID,
                    CreatedAt = order.CreatedAt,
                    Image = imageData
                });

            }
            return Ok(ordersModel);
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _context.Services.ToListAsync();

            if (services == null)
            {
                return NotFound(new { message = "Нет записей" });
            }

            return Ok(services);
        }

        [HttpPut("updateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] RepairOrders order)
        {
            var orders = await _context.RepairOrders.FindAsync(id);

            if (orders == null)
            {
                return NotFound(new { message = "Отчет не найден" });
            }

            orders.StatusID = order.StatusID;
            orders.MasterID = order.MasterID;
            orders.Price = order.Price;

            if (order.StatusID == 1)
            {
                orders.AcceptedAt = DateTime.UtcNow;
            }
            else if(order.StatusID == 2)
            {
                if (order.AcceptedAt == null)
                    orders.AcceptedAt = DateTime.UtcNow;

                orders.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                if (order.AcceptedAt == null)
                    orders.AcceptedAt = DateTime.UtcNow;

                if (order.CompletedAt == null)
                    orders.CompletedAt = DateTime.UtcNow;

                orders.DeliveredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Отчет обновлён" });
        }
    }

}
