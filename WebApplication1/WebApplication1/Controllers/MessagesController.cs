using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("chats")]
        public async Task<IActionResult> GetAllChats([FromBody] int UserID)
        {
            var chats = await (
                from RepairOrders in _context.RepairOrders
                where (RepairOrders.ClientID == UserID || RepairOrders.MasterID == UserID) && RepairOrders.MasterID != null
                join user in _context.Users on RepairOrders.MasterID equals user.UserID
                orderby RepairOrders.OrderID descending
                select new Chat
                {
                    OrderID = RepairOrders.OrderID,
                    MasterID = RepairOrders.MasterID,
                    Name = user.Middlename + " " + user.Firstname,
                    Description = RepairOrders.Name,
                    StatusID = RepairOrders.StatusID,
                    Price = RepairOrders.Price
                })
                .ToListAsync();

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "order");

            if (chats == null)
            {
                return NotFound(new { message = "Нет записей" });
            }
            foreach (var chat in chats)
            {
                var image = await _context.RepairOrderImages.FirstOrDefaultAsync(u => u.OrderID == chat.OrderID);

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

                chat.Image = imageData;
            }


            return Ok(chats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessages(int id)
        {
            var messages = await _context.Messages
                .Where(u => u.OrderID == id)
                .OrderBy(t => t.MessageID)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("addMessage")]
        public async Task<IActionResult> AddMessage([FromBody] Messages message)
        {
            var messageModel = new Messages
            {
                OrderID = message.OrderID,
                SenderID = message.SenderID,
                RecipientID = message.RecipientID,
                MessageText = message.MessageText,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(messageModel);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
