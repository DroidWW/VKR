using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;
using MauiApp1.Models;
using MauiApp1.Storage;
using MauiApp1.Views;

namespace MauiApp1.Views;

public partial class MasterOrderPage : ContentPage
{
    public ICommand RefreshCommand { get; }
    private readonly HttpClient _httpClient;
	public MasterOrderPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
        RefreshCommand = new Command(async () => await ShowItemsForOrders(true));
        BindingContext = this;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ShowItemsForOrders(false);
    }
    private async Task ShowItemsForOrders(bool isRefresh = false)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/repairOrders/orders");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<RepairOrders>>(json, new JsonSerializerOptions { WriteIndented = true });
                CollectionOrder.ItemsSource = orders;
            }
            else
            {
                Debug.WriteLine($"Ошибка загрузки заказов");
                return;
            }
            if (isRefresh)
            {
                OrderRefreshView.IsRefreshing = false;
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
    }
    private async void ButtonToOrderPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OrderPage(_httpClient));
    }
    private async void ButtonToOrder(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RepairOrders selectedOrder)
            await Navigation.PushAsync(new EditOrderPage(selectedOrder, _httpClient));

        ((CollectionView)sender).SelectedItem = null;
    }
}