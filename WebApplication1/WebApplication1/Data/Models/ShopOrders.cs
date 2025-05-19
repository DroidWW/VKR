using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication1.Data.Models
{
    public class ShopOrders
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShopOrderID { get; set; }
        public int UserID { get; set; }
        public int Price { get; set; }
        public bool IsSold { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ?PaidAt { get; set; }
    }
}
