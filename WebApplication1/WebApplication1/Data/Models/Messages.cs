using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data.Models
{
    public class Messages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageID { get; set; }
        public int OrderID { get; set; }
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
    }
    public class Chat
    {
        public int ?MasterID { get; set; }
        public int OrderID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; }
        public int ?Price { get; set; }
        public byte[] ?Image { get; set; }
    }
}
