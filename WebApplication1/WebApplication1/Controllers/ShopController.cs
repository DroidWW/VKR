using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Where(u => u.IsOrdered == false)
                .OrderByDescending(u => u.ProductID)
                .ToListAsync();

            if (products == null)
            {
                return NotFound(new { message = "Нет записей" });
            }

            var productsModel = new List<UpdateProducts>();
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");
            
            if (!Directory.Exists(uploadFolder))
            {
                return NotFound(new { message = "Папка с изображениями не найдена" });
            }

            foreach (var product in products)
            {
                var image = await _context.ProductImages.FirstOrDefaultAsync(u => u.ProductID == product.ProductID);

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

                productsModel.Add(new UpdateProducts
                {
                    ProductID = product.ProductID,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Image = imageData
                });

            }

            return Ok(productsModel);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] Products product)
        {

            var newProduct = new Products
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            return Ok(new { productID = newProduct.ProductID });
        }

        [HttpPost("images")]
        public async Task<IActionResult> Profile([FromBody] ImagesUploadForProducts products)
        {
            if (products == null || products.Data == null || products.Data.Length == 0)
                return BadRequest("Нет данных");

            var product = await _context.Products.FirstOrDefaultAsync(u => u.ProductID == products.ProductID);
            if (product == null)
                return NotFound(new { message = "Нет данных" });

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, products.Filename);

            try
            {
                await System.IO.File.WriteAllBytesAsync(filePath, products.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при сохранении файла", error = ex.Message });
            }

            var productImage = new ProductImages
            {
                ProductID = products.ProductID,
                ImageURL = products.Filename
            };

            _context.ProductImages.Update(productImage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Изображение успешно загрузилось" });

        }

        [HttpPost("addShopOrder")]
        public async Task<IActionResult> AddShopOrder([FromBody] ShopOrders shopOrder)
        {

            var newShopOrder= new ShopOrders
            {
                UserID = shopOrder.UserID,
                Price = shopOrder.Price,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ShopOrders.AddAsync(newShopOrder);
            await _context.SaveChangesAsync();

            return Ok(new { shopOrderID = newShopOrder.ShopOrderID });
        }

        [HttpPost("shopOrders/{UserType}")]
        public async Task<IActionResult> GetShopOrders(int UserType,[FromBody] int id)
        {
            List<ShopOrders> shopOrders = null;

            if (UserType == 1)
            {
                shopOrders = await _context.ShopOrders
                    .Where(n => n.UserID == id)
                    .ToListAsync();
            }
            else
            {
                shopOrders = await _context.ShopOrders.ToListAsync();
            }


            if (shopOrders == null)
            {
                return NotFound(new { message = "Нет заказов" });
            }

            return Ok(shopOrders);
        }

        [HttpPost("products")]
        public async Task<IActionResult> GetAllProducts([FromBody] int id)
        {
            var products = await _context.Products
                .Where(u => u.IsOrdered == true && u.ShopOrderID == id)
                .OrderByDescending(u => u.ShopOrderID)
                .ToListAsync();

            if (products == null)
            {
                return NotFound(new { message = "Нет записей" });
            }

            var productsModel = new List<UpdateProducts>();
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");

            if (!Directory.Exists(uploadFolder))
            {
                return NotFound(new { message = "Папка с изображениями не найдена" });
            }

            foreach (var product in products)
            {
                var image = await _context.ProductImages.FirstOrDefaultAsync(u => u.ProductID == product.ProductID);

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

                productsModel.Add(new UpdateProducts
                {
                    ProductID = product.ProductID,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Image = imageData
                });

            }

            return Ok(productsModel);
        }

        [HttpDelete("deleteShopOrder/{id}")]
        public async Task<IActionResult> DeleteShopOrder(int id)
        {
            var shopOrder =  _context.ShopOrders.FirstOrDefault(u => u.ShopOrderID == id);
            if (shopOrder == null)
            {
                return NotFound(new { message = "Заказ не найден" });
            }

            _context.ShopOrders.Remove(shopOrder);

            var products = await _context.Products
                .Where(u => u.ShopOrderID == id)
                .OrderByDescending(u => u.ShopOrderID)
                .ToListAsync();

            if (products == null)
            {
                return NotFound(new { message = "Товары не найдены" });
            }

            foreach (var product in products)
            {
                product.IsOrdered = false;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Товары обновлены" });
        }

        [HttpPut("updateShopOrder")]
        public async Task<IActionResult> UpdateShopOrder([FromBody] int id)
        {
            var shopOrder = _context.ShopOrders.FirstOrDefault(u => u.ShopOrderID == id);

            if (shopOrder == null)
            {
                return NotFound(new { message = "Заказ не найден" });
            }

            shopOrder.IsSold = true;
            shopOrder.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Заказ обновлён" });
        }

        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateProducts product)
        {
            var products = await _context.Products.FindAsync(id);

            if (products == null)
            {
                return NotFound(new { message = "Отчет не найден" });
            }

            products.Name = product.Name;
            products.Description = product.Description;
            products.Price = product.Price;
            products.IsOrdered = product.IsOrdered;
            products.ShopOrderID = product.ShopOrderID;

            await _context.SaveChangesAsync();

            if (product.Image != null)
            {
                var image = await _context.ProductImages.FirstOrDefaultAsync(u => u.ProductID == id);

                if (image == null)
                    return NotFound(new { message = "Нет данных" });

                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");

                if (!Directory.Exists(uploadFolder))
                    return NotFound(new { message = "Нет данных" });


                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                try
                {
                    await System.IO.File.WriteAllBytesAsync(filePath, product.Image);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Ошибка при сохранении файла", error = ex.Message });
                }
            }

            return Ok(new { message = "Товар обновлён" });
        }

        [HttpDelete("deleteProduct/{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var product = _context.Products.FirstOrDefault(u => u.ProductID == id);
            if (product == null)
            {
                return NotFound(new { message = "Товар не найден" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            var image = _context.ProductImages.FirstOrDefault(u => u.ProductID == id);
            if (image == null)
            {
                return NotFound(new { message = "Товар не найден" });
            }
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            if (image.ImageURL != null)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");

                var filePath = Path.Combine(uploadFolder, image.ImageURL);

                System.IO.File.Delete(filePath);
            }

            return Ok(new { message = "Товар удален" });
        }

    }
}
