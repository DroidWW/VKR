
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.Json.Serialization;

namespace MauiApp1.Storage
{
    public class Orders
    {
        [JsonPropertyName("clientID")]
        public int ClientID { get; set; }
        [JsonPropertyName("servicesID")]
        public int ServicesID { get; set; }
        [JsonPropertyName("name")]
        [MaxLength(100)]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description {  get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class RepairOrders
    {
        [JsonPropertyName("orderID")]
        public int OrderID {  get; set; }
        [JsonPropertyName("clientID")]
        public int ClientID { get; set; }
        [JsonPropertyName("masterID")]
        public int ?MasterID { get; set; }
        [JsonPropertyName("servicesID")]
        public int ServicesID { get; set; }
        [JsonPropertyName("name")]
        [MaxLength(100)]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("statusID")]
        public int StatusID { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("acceptedAt")]
        public DateTime ?AcceptedAt { get; set; }
        [JsonPropertyName("completedAt")]
        public DateTime ?CompletedAt { get; set; }
        [JsonPropertyName("deliveredAt")]

        public DateTime ?DeliveredAt { get; set; }
        [JsonPropertyName("price")]
        public int ?Price { get; set; }
        [JsonPropertyName("image")]
        public byte[] ?Image { get; set; }

        [JsonIgnore]
        public ImageSource ImageSource
        {
            get
            {
                if (Image == null || Image.Length == 0)
                    return "item_icon.png";
                return ImageSource.FromStream(() => new MemoryStream(Image));
            }
        }
    }

    public class Chat
    {
        [JsonPropertyName("masterID")]
        public int ?MasterID { get; set; }
        [JsonPropertyName("orderID")]
        public int OrderID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("statusID")]
        public int StatusID { get; set; }
        [JsonPropertyName("price")]
        public int ?Price { get; set; }
        [JsonPropertyName("image")]
        public byte[] ?Image { get; set; }

        [JsonIgnore]
        public ImageSource ImageSource
        {
            get
            {
                if (Image == null || Image.Length == 0)
                    return "item_icon.png";
                return ImageSource.FromStream(() => new MemoryStream(Image));
            }
        }
    }

    public class Services
    {
        [JsonPropertyName("serviceID")]
        public int ServiceID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
