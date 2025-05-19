using MauiApp1.Storage;
using System.Text.Json.Serialization;

namespace MauiApp1.Models
{
    public class Messages
    {
        [JsonPropertyName("messageID")]
        public int MessageID { get; set; }
        [JsonPropertyName("orderID")]
        public int OrderID { get; set; }
        [JsonPropertyName("senderID")]
        public int SenderID { get; set; }
        [JsonPropertyName("recipientID")]
        public int RecipientID { get; set; }
        [JsonPropertyName("messageText")]
        public string MessageText { get; set; }
        [JsonPropertyName("sentAt")]
        public DateTime SentAt { get; set; }
    }
    public class MessageModel
    {
        [JsonPropertyName("messageID")]
        public int MessageID { get; set; }
        [JsonPropertyName("orderID")]
        public int OrderID { get; set; }
        [JsonPropertyName("senderID")]
        public int SenderID { get; set; }
        [JsonPropertyName("recipientID")]
        public int RecipientID { get; set; }
        [JsonPropertyName("messageText")]
        public string MessageText { get; set; }
        [JsonPropertyName("sentAt")]
        public DateTime SentAt { get; set; }

        [JsonIgnore]
        public LayoutOptions HorizOpt
        {
            get
            {
                if (SenderID == UserManager.UserId)
                    return LayoutOptions.End;
                return LayoutOptions.Start;
            }
        }
        [JsonIgnore]
        public Color Color
        {
            get
            {
                if (SenderID == UserManager.UserId)
                    return Color.FromArgb("#92D36E");
                return Color.FromArgb("#DDD");
            }
        }
    }
}
