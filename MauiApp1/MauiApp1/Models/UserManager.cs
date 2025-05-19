using System.Text.Json.Serialization;

namespace MauiApp1.Storage
{
    public static class UserManager
    {
        public static int UserId {  get; set; }
        public static string CurrentLogin { get; set; } = string.Empty;
        public static string Firstname { get; set; } = string.Empty;
        public static string Middlename { get; set; } = string.Empty;
        public static string Lastname { get; set; } = string.Empty;
        public static int UserType { get; set; }
        public static byte[] ?ProfileImage { get; set; }
    }

    public class Users
    {
        [JsonPropertyName("userID")]
        public int UserId { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("firstname")]
        public string Firstname { get; set; }
        [JsonPropertyName("middlename")]
        public string Middlename { get; set; }
        [JsonPropertyName("lastname")]
        public string Lastname { get; set; }
        [JsonPropertyName("userType")]
        public int UserType { get; set; }
    }
}
