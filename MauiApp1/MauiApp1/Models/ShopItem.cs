using Microsoft.Maui.Graphics.Text;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace MauiApp1.Models
{
    public static class BasketItems
    {
        public static int Price { get; set; } = 0;
        public static int Count { get; set; } = 0;
        public static int[] ItemsID { get; set; } = new int[0];

    }
    public class ShopItem
    {
        [JsonPropertyName("productID")]
        public int ProductID { get; set; }
        [JsonPropertyName("name")]
        [MaxLength(100)]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("price")]
        public int Price { get; set; }
        [JsonPropertyName("isOrdered")]
        public bool IsOrdered { get; set; }
        [JsonPropertyName("shopOrderID")]
        public int ?ShopOrderID { get; set; }
        [JsonPropertyName("image")]
        public byte[] ?Image { get; set; }
        public bool IsPlus { get; set; } = true;

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

        [JsonIgnore]
        public string IconSource
        {
            get => IsPlus ? "plus_icon.png" : "minus_icon.png";
        }
    }

    public class ShopOrders
    {
        [JsonPropertyName("shopOrderID")]
        public int ShopOrderID {  get; set; }
        [JsonPropertyName("userID")]
        public int UserID { get; set; }
        [JsonPropertyName("price")]
        public int Price { get; set; }
        [JsonPropertyName("isSold")]
        public bool IsSold { get; set; }

        [JsonIgnore]
        public string Color
        {
            get
            {
                if (!IsSold)
                    return "#e74c3c";
                return "#92D36E";
            }
        }

        [JsonIgnore]
        public string Text
        {
            get
            {
                if (!IsSold)
                    return "Не оплачен";
                return "Оплачен";
            }
        }
    }
}
