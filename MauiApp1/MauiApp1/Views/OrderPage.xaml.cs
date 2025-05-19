using MauiApp1.Storage;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MauiApp1;

public partial class OrderPage : ContentPage
{
    public class OrderResponse
    {
        [JsonPropertyName("orderID")]
        public int OrderID { get; set; }
    }

    private byte[] _selectedImagesBytes = null;
    private readonly HttpClient _httpClient;

    public OrderPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DatePicker.MinimumDate = DateTime.Today;
        DatePicker.MaximumDate = DateTime.Today.AddMonths(1);
        GetServices();
    }

    private async void GetServices()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/repairOrders/services");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var services = JsonSerializer.Deserialize<List<Services>>(json, new JsonSerializerOptions { WriteIndented = true });

                foreach (var service in services)
                    ServicePicker.Items.Add(service.Name);
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось загрузить услуги", "Ок");
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
    }
    private void ActivityIndicatorRunning()
    {
        ButtonForAccept.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivivtyIndicatorStopping()
    {
        ButtonForAccept.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

    private async void ButtonAddOrder(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        if (ServicePicker.SelectedItem == null
            || DatePicker.Date == default
            || string.IsNullOrWhiteSpace(Title.Text)
            || string.IsNullOrWhiteSpace(Description.Text))
        {
            await DisplayAlert("Ошибка!", "Не все поля заполнены!", "Ок");
            ActivivtyIndicatorStopping();
            return;
        }

        var orderModel = new RepairOrders
        {
            ClientID = UserManager.UserId,
            ServicesID = ServicePicker.SelectedIndex,
            Name = Title.Text,
            Description = Description.Text,
            CreatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(orderModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("/api/repairOrders/add", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var orderResponse = JsonSerializer.Deserialize<OrderResponse>(responseContent);

                if (orderResponse != null)
                {
                    int orderId = orderResponse.OrderID;

                    if (_selectedImagesBytes != null && _selectedImagesBytes.Length > 0)
                    {
                        var imageRequest = new
                        {
                            OrderID = orderId,
                            Filename = $"order_{orderId}.jpg",
                            Data = _selectedImagesBytes
                        };

                        var imageJson = JsonSerializer.Serialize(imageRequest);
                        var imageContent = new StringContent(imageJson, Encoding.UTF8, "application/json");

                        var imageResponse = await _httpClient.PostAsync("/api/repairOrders/images", imageContent);

                        if (!imageResponse.IsSuccessStatusCode)
                        {
                            await DisplayAlert("Ошибка", "Не удалось загрузить фото", "Ок");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось получить ID заказа", "Ок");
                    return;
                }

                OrderImage.Source = "item_icon.png";
                ServicePicker.SelectedItem = null;
                DatePicker.Date = default;
                Title.Text = "";
                Description.Text = "";
                _selectedImagesBytes = null;
                await DisplayAlert("Успех", "Заказ отправлен", "Ок");
            }
            else
            {
                await DisplayAlert("Ошибка", "Заказ не отправлен", "Ок");
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
        OrderImage.Source = ImageSource.FromStream(() => new MemoryStream(_selectedImagesBytes));
    }
}