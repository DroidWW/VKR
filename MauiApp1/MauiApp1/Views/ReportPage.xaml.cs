using MauiApp1.Models;
using MauiApp1.Storage;
using System.Text;
using System.Text.Json;

namespace MauiApp1.Views;

public partial class ReportPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private byte[] _selectedImagesBytes = null;
    private bool _isEditing = false;
    private Reports _report;
    public ReportPage(Reports report, HttpClient httpClient) : base()
	{
		InitializeComponent();
        _httpClient = httpClient;
        BindingContext = report;
        _report = report;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (UserManager.UserType != 1)
        {
            ForMasterField.IsVisible = true;
        }
        EditImage.Source = DefaultImage.Source;
    }

    private void ActivityIndicatorRunning()
    {
        DeleteButton.IsEnabled = false;
        EditButton.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivityIndicatorStopping()
    {
        DeleteButton.IsEnabled = true;
        EditButton.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

    private async void EditReport(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        if (_isEditing)
        {
            if(EditImage.Source != DefaultImage.Source
                || EditName.Text != DefaultName.Text
                || EditDescription.Text != DefaultDescriptionText.Text)
            {
                var answer = await DisplayAlert("Подтверждение", "Точно ли хотите поменять данные?", "Да", "Нет");
                if (!answer)
                {
                    EditImage.Source = DefaultImage.Source;
                    EditName.Text = DefaultName.Text;
                    EditDescription.Text = DefaultDescriptionText.Text;
                }
                else
                {
                    _report.Name = EditName.Text;
                    _report.Description = EditDescription.Text;
                    _report.Image = _selectedImagesBytes;

                    var json = JsonSerializer.Serialize(_report);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        var response = await _httpClient.PutAsync($"/api/reports/updateReport/{_report.ReportID}", content);

                        if (response.IsSuccessStatusCode)
                        {
                            DefaultImage.Source = EditImage.Source;
                            DefaultName.Text = EditName.Text;
                            DefaultDescriptionText.Text = EditDescription.Text;
                        }
                        else
                        {
                            await DisplayAlert("Ошибка", "Не удалось обновить данные", "Ок");
                            EditImage.Source = DefaultImage.Source;
                            EditName.Text = DefaultName.Text;
                            EditDescription.Text = DefaultDescriptionText.Text;
                            return;
                        }
                    }
                    catch
                    {
                        await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
                    }
                    finally
                    {
                        ActivityIndicatorStopping();
                    }
                }
            }
        }

        DefaultImage.IsVisible = _isEditing;
        DefaultName.IsVisible = _isEditing;
        DefaultDescription.IsVisible = _isEditing;

        EditImage.IsVisible = !_isEditing;
        EditName.IsVisible = !_isEditing;
        EditDescription.IsVisible = !_isEditing;

        if (!_isEditing)
            EditButton.Text = "Сохранить";
        else
            EditButton.Text = "Редактировать";
        _isEditing = !_isEditing;
        ActivityIndicatorStopping();
    }

    private async void DeleteReport(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/reports/deleteReport/{_report.ReportID}");
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успешно", "Отчет удален", "Ок");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Отчет не удален", "Ок");
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
        finally
        {
            ActivityIndicatorStopping();
        }
    }

    private async void OnUpdateImageClicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo == null)
            return;

        using var sourceStream = await photo.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream);

        _selectedImagesBytes = memoryStream.ToArray();
        DefaultImage.Source = ImageSource.FromStream(() => new MemoryStream(_selectedImagesBytes));
        EditImage.Source = ImageSource.FromStream(() => new MemoryStream(_selectedImagesBytes));
    }
}