using MauiApp1.Storage;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MauiApp1.Views;

public partial class EditOrderPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private byte[] _selectedImagesBytes = null;
    private RepairOrders _order;

    public EditOrderPage(RepairOrders order, HttpClient httpClient) : base()
	{
		InitializeComponent();
        _httpClient = httpClient;
        BindingContext = order;
        _order = order;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
    }

    private void ActivityIndicatorRunning()
    {
        ButtonForAcceptEdit.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivivtyIndicatorStopping()
    {
        ButtonForAcceptEdit.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }
    private async void EditButton(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();

        if (ServicePicker.SelectedIndex + 1 == _order.StatusID)
        {
            await DisplayAlert("Ошибка!", "Статус не изменен!", "Ок");
            ActivivtyIndicatorStopping();
            return;
        }

        if (ServicePicker.SelectedIndex + 1 < _order.StatusID)
        {
            await DisplayAlert("Ошибка!", "Статус не может быть понижен!", "Ок");
            ActivivtyIndicatorStopping();
            return;
        }

        _order.StatusID = ServicePicker.SelectedIndex + 1;
        _order.MasterID = UserManager.UserId;

        var json = JsonSerializer.Serialize(_order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PutAsync($"/api/repairOrders/updateStatus/{_order.OrderID}", content);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успешно", "Статус изменен", "Ок");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Статус не изменен", "Ок");
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
}