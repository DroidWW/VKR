using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data.Models
{
    public class ProductImages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageID { get; set; }
        public int ProductID { get; set; }
        [MaxLength(255)]
        public string ImageURL { get; set; }
    }
}
