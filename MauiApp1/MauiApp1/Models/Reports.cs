using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;


namespace MauiApp1.Models
{
    public class Reports: INotifyPropertyChanged
    {
        [JsonPropertyName("reportID")]
        public int ReportID { get; set; }
        [JsonPropertyName("orderID")]
        public int ?OrderID { get; set; }
        [JsonPropertyName("masterID")]
        public int MasterID { get; set; }
        [JsonPropertyName("name")]
        [MaxLength(25)]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
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

         [JsonPropertyName("reportsList")]
        public ObservableCollection<ReportsComments> ?ReportsList { get; set; } = new ObservableCollection<ReportsComments>();
        private int _likeCount;
        private int _dislikeCount;
        [JsonPropertyName("likeCount")]
        public int LikeCount
        {
            get => _likeCount;
            set
            {
                if (_likeCount != value)
                {
                    _likeCount = value;
                    OnPropertyChanged();
                }
            }
        }
        [JsonPropertyName("dislikeCount")]
        public int DislikeCount
        {
            get => _dislikeCount;
            set
            {
                if (_dislikeCount != value)
                {
                    _dislikeCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CommentsCount
        {
            get
            {
                return ReportsList?.Count ?? 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class LikesDislikes
    {
        [JsonPropertyName("reportID")]
        public int ReportID { get; set; }
        [JsonPropertyName("userID")]
        public int UserID { get; set; }
        [JsonPropertyName("isLiked")]
        public bool IsLiked { get; set; }
        [JsonPropertyName("isDisliked")]
        public bool IsDisliked { get; set; }
    }

    public class ReportsComments
    {
        [JsonPropertyName("reportID")]
        public int ReportID { get; set; }
        [JsonPropertyName("userID")]
        public int UserID { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

    }
}
