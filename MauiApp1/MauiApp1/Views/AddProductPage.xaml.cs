using MauiApp1.Models;
using MauiApp1.Storage;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiApp1.Views;

public partial class AddProductPage : ContentPage
{
    public class ProductResponse
    {
        [JsonPropertyName("productID")]
        public int ProductID { get; set; }
    }

    private byte[] _selectedImagesBytes = null;
    private readonly HttpClient _httpClient;

    public AddProductPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
    }
    private void ActivityIndicatorRunning()
    {
        ButtonForAddProduct.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivivtyIndicatorStopping()
    {
        ButtonForAddProduct.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }
    private async void ButtonAddProduct(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        int.TryParse(Price.Text, out int price);

        if (string.IsNullOrWhiteSpace(Name.Text)
            || string.IsNullOrWhiteSpace(Description.Text)
            || string.IsNullOrWhiteSpace(Price.Text))
        {
            await DisplayAlert("Ошибка!", "Не все поля заполнены!", "Ок");
            ActivivtyIndicatorStopping();
            return;
        }

        var productModel = new ShopItem
        {
            Name = Name.Text,
            Description = Description.Text,
            Price = price,
        };

        var json = JsonSerializer.Serialize(productModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync("/api/shop/add", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var reportResponse = JsonSerializer.Deserialize<ProductResponse>(responseContent);
                
                if (reportResponse != null)
                {
                    int productId = reportResponse.ProductID;
                    if (_selectedImagesBytes != null && _selectedImagesBytes.Length > 0)
                    {
                        var imageRequest = new
                        {
                            ProductID = productId,
                            Filename = $"product_{productId}.jpg",
                            Data = _selectedImagesBytes
                        };

                        var imageJson = JsonSerializer.Serialize(imageRequest);
                        var imageContent = new StringContent(imageJson, Encoding.UTF8, "application/json");

                        var imageResponse = await _httpClient.PostAsync("/api/shop/images", imageContent);

                        if (!imageResponse.IsSuccessStatusCode)
                        {
                            await DisplayAlert("Ошибка", "Не удалось загрузить фото", "Ок");
                        }

                    }
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось получить ID товара", "Ок");
                    return;
                }

                ProductImage.Source = "item_icon.png";
                Name.Text = "";
                Description.Text = "";
                Price.Text = "";
                _selectedImagesBytes = null;
                await DisplayAlert("Успех", "Товар добавлен", "Ок");
            }
            else
            {
                await DisplayAlert("Ошибка", "Товар не добавлен", "Ок");
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
        finally
        {
            ActivivtyIndicatorStopping();
        }

    }

    private async void LoadImage(object sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo == null)
            return;

        using var sourceStream = await photo.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream);

        _selectedImagesBytes = memoryStream.ToArray();
        ProductImage.Source = ImageSource.FromStream(() => new MemoryStream(_selectedImagesBytes));
    }
}