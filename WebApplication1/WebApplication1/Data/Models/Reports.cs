using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data.Models
{
    public class Reports
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportID {  get; set; }
        public int ?OrderID { get; set; }
        public int MasterID { get; set; }
        [MaxLength(25)]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class ReportsImages
    {
        public int ReportID { get; set; }
        public int ?OrderID { get; set; }
        public int MasterID { get; set; }
        [MaxLength(25)]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[] ?Image { get; set; }
        public int LikeCount {  get; set; }
        public int DislikeCount { get; set; }
        public List<ReportsComments> ?ReportsList { get; set; }

    }

    public class LikesDislikes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ReportID { get; set; }
        public int UserID { get; set; }
        public bool IsLiked { get; set; }
        public bool IsDisliked { get; set; }
    }
    public class ReportsComments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentID {  get; set; }
        public int ReportID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }
    }
}
