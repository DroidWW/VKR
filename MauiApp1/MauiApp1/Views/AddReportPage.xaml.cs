using MauiApp1.Models;
using MauiApp1.Storage;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace MauiApp1.Views;

public partial class AddReportPage : ContentPage
{
    public class ReportResponse
    {
        [JsonPropertyName("reportID")]
        public int ReportID { get; set; }
    }

    private byte[] _selectedImagesBytes = null;
    private readonly HttpClient _httpClient;
	public AddReportPage(HttpClient httpClient)
	{
		InitializeComponent();
		_httpClient = httpClient;
	}

    private void ActivityIndicatorRunning()
    {
        ButtonForAddReport.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivivtyIndicatorStopping()
    {
        ButtonForAddReport.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

    private async void ButtonAddReport(object sender, EventArgs e)
	{
        ActivityIndicatorRunning();

        if (string.IsNullOrWhiteSpace(Name.Text)
            || string.IsNullOrWhiteSpace(Description.Text))
        {
            await DisplayAlert("Ошибка!", "Не все поля заполнены!", "Ок");
            ActivivtyIndicatorStopping();
            return;
        }

        var reportModel = new Reports
        {
            MasterID = UserManager.UserId,
            Name = Name.Text,
            Description = Description.Text,
            CreatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(reportModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync("/api/reports/add", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var reportResponse = JsonSerializer.Deserialize<ReportResponse>(responseContent);

                if (reportResponse != null)
                {
                    int reportId = reportResponse.ReportID;
                    if (_selectedImagesBytes != null && _selectedImagesBytes.Length > 0)
                    {
                        var imageRequest = new
                        {
                            ReportID = reportId,
                            Filename = $"report_{reportId}.jpg",
                            Data = _selectedImagesBytes
                        };

                        var imageJson = JsonSerializer.Serialize(imageRequest);
                        var imageContent = new StringContent(imageJson, Encoding.UTF8, "application/json");

                        var imageResponse = await _httpClient.PostAsync("/api/reports/images", imageContent);

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

                ReportImage.Source = "item_icon.png";
                Name.Text = "";
                Description.Text = "";
                _selectedImagesBytes = null;
                await DisplayAlert("Успех", "Отчет отправлен", "Ок");
            }
            else
            {
                await DisplayAlert("Ошибка", "Отчет не отправлен", "Ок");
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
        ReportImage.Source = ImageSource.FromStream(() => new MemoryStream(_selectedImagesBytes));
    }
}