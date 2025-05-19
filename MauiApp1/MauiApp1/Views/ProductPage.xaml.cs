using MauiApp1.Models;
using MauiApp1.Storage;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MauiApp1.Views;

public partial class ProductPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private byte[] _selectedImagesBytes = null;
    private bool _isEditing = false;
    private ShopItem _product;
    public ProductPage(ShopItem product, HttpClient httpClient) : base()
	{
		InitializeComponent();
        _httpClient = httpClient;
        BindingContext = product;
        _product = product;
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
        EditButton.IsEnabled = false;
        DeleteButton.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivivtyIndicatorStopping()
    {
        EditButton.IsEnabled = true;
        DeleteButton.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

    private async void EditProduct(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        if (_isEditing)
        {
            if (EditImage.Source != DefaultImage.Source
                || EditName.Text != DefaultName.Text
                || EditDescription.Text != DefaultDescriptionText.Text
                || EditPrice.Text != DefaultPrice.Text)
            {
                var answer = await DisplayAlert("Подтверждение", "Точно ли хотите поменять данные?", "Да", "Нет");
                if (!answer)
                {
                    EditImage.Source = DefaultImage.Source;
                    EditName.Text = DefaultName.Text;
                    EditDescription.Text = DefaultDescriptionText.Text;
                    EditPrice.Text = DefaultPrice.Text;

                }
                else
                {
                    _product.Name = EditName.Text;
                    _product.Description = EditDescription.Text;
                    _product.Image = _selectedImagesBytes;

                    var json = JsonSerializer.Serialize(_product);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        var response = await _httpClient.PutAsync($"/api/shop/updateProduct/{_product.ProductID}", content);
                        var responseBody = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            DefaultImage.Source = EditImage.Source;
                            DefaultName.Text = EditName.Text;
                            DefaultDescriptionText.Text = EditDescription.Text;
                            DefaultPrice.Text = EditPrice.Text;
                        }
                        else
                        {
                            await DisplayAlert("Ошибка", "Не удалось обновить данные", "Ок");
                            EditImage.Source = DefaultImage.Source;
                            EditName.Text = DefaultName.Text;
                            EditDescription.Text = DefaultDescriptionText.Text;
                            EditPrice.Text = DefaultPrice.Text;
                            return;
                        }
                    }
                    catch
                    {
                        await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
                    }
                }
            }
        }

        DefaultImage.IsVisible = _isEditing;
        DefaultName.IsVisible = _isEditing;
        DefaultDescription.IsVisible = _isEditing;
        DefaultPrice.IsVisible = _isEditing;

        EditImage.IsVisible = !_isEditing;
        EditName.IsVisible = !_isEditing;
        EditDescription.IsVisible = !_isEditing;
        EditPrice.IsVisible = !_isEditing;

        if (!_isEditing)
            EditButton.Text = "Сохранить";
        else
            EditButton.Text = "Редактировать";
        _isEditing = !_isEditing;
        ActivivtyIndicatorStopping();
    }

    private async void DeleteProduct(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/shop/deleteProduct/{_product.ProductID}");
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успешно", "Товар удален", "Ок");
                await Navigation.PopAsync();
            }
            {
                await DisplayAlert("Ошибка", "Товар не удален", "Ок");
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