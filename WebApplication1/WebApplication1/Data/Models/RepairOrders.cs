using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data.Models
{
    public class RepairOrders
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }
        public int ClientID {  get; set; }
        public int ?MasterID {  get; set; }
        public int ServicesID { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ?AcceptedAt { get; set; }
        public DateTime ?CompletedAt { get; set; }
        public DateTime ?DeliveredAt { get; set; }
        public int ?Price {  get; set; }

    }
    public class UpdateRepairOrders
    {
        public int OrderID { get; set; }
        public int ClientID {  get; set; }
        public int ?MasterID {  get; set; }
        public int ServicesID { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ?AcceptedAt { get; set; }
        public DateTime ?CompletedAt { get; set; }
        public DateTime ?DeliveredAt { get; set; }
        public int ?Price {  get; set; }
        public byte[] ?Image { get; set; }

    }

    public class Services
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceID { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
