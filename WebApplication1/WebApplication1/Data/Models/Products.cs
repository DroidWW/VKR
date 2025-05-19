using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data.Models
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID {  get; set; }
        [MaxLength(100)]
        public string Name {  get; set; }
        public string Description {  get; set; }
        public int Price {  get; set; }
        public bool IsOrdered {  get; set; }
        public int? ShopOrderID {  get; set; }

    }
    public class UpdateProducts
    {
        public int ProductID { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public byte[]? Image { get; set; }
        public bool IsOrdered { get; set; }
        public int? ShopOrderID { get; set; }

    }

}
