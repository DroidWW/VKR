using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID {  get; set; }
        [MaxLength(25)]
        public string Username { get; set; }
        public string Password { get; set; }
        [MaxLength(25)]
        public string Firstname { get; set;}
        [MaxLength(25)]
        public string Lastname { get; set;}
        [MaxLength(25)]
        public string Middlename { get; set;}
        public int UserType { get; set; }

        public DateTime RegistrationDate { get; set;}
    }
    public class UpdateUsers
    {
        public int UserID { get; set; }
        [MaxLength(25)]
        public string Username { get; set; }
        public string Password { get; set; }
        [MaxLength(25)]
        public string Firstname { get; set; }
        [MaxLength(25)]
        public string Lastname { get; set; }
        [MaxLength(25)]
        public string Middlename { get; set; }
        public string ImageURL {  get; set; }   
        public byte[] Data { get; set; }
    }
}
