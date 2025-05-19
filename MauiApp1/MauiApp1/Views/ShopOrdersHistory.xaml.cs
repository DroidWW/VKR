using MauiApp1.Models;
using MauiApp1.Storage;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace MauiApp1.Views;

public partial class ShopOrdersHistory : ContentPage
{
    public ICommand RefreshCommand { get; }
    private readonly HttpClient _httpClient;
    private int _shopOrderID;
    public ShopOrdersHistory(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        RefreshCommand = new Command(async () => await ShowShopOrders(true));
        BindingContext = this;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ShowShopOrders(false);
    }

    private async Task ShowShopOrders(bool isRefresh = false)
    {
        try
        {
            var json = JsonSerializer.Serialize(UserManager.UserId);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/api/shop/shopOrders/{UserManager.UserType}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseContent = JsonSerializer.Deserialize<List<ShopOrders>>(responseJson, new JsonSerializerOptions { WriteIndented = true });
                ShopOrdersItems.ItemsSource = responseContent;
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
    }

    private void ActivityIndicatorRunning()
    {
        ButtonForCloseShopOrder.IsEnabled = false;
        ButtonForPayShopOrder.IsEnabled = false;
        ButtonForDeleteShopOrder.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivityIndicatorStopping()
    {
        ButtonForCloseShopOrder.IsEnabled = true;
        ButtonForPayShopOrder.IsEnabled = true;
        ButtonForDeleteShopOrder.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

    private async void ButtonToOrder(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ShopOrders selectedOrder)
        {

            if (selectedOrder.IsSold || UserManager.UserType > 1)
            {
                ButtonForDeleteShopOrder.IsVisible = false;
                ButtonForPayShopOrder.IsVisible = false;
            }
            else
            {
                ButtonForDeleteShopOrder.IsVisible = true;
                ButtonForPayShopOrder.IsVisible = true;
            }

            _shopOrderID = selectedOrder.ShopOrderID;
            try
            {
                var json = JsonSerializer.Serialize(selectedOrder.ShopOrderID);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/shop/products/", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseContent = JsonSerializer.Deserialize<List<ShopItem>>(responseJson, new JsonSerializerOptions { WriteIndented = true });
                    OrderProductsList.ItemsSource = responseContent;
                    ProductsPanel.IsVisible = true;
                }
            }
            catch
            {
                await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
            }

        }

        ((CollectionView)sender).SelectedItem = null;

    }

    private async void OnCloseProductsPanel(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        ProductsPanel.IsVisible = false;
        OrderProductsList.ItemsSource = null;
        ActivityIndicatorStopping();
    }

    private async void OnDeleteShopOrder(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/shop/deleteShopOrder/{_shopOrderID}");
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успешно", "Заказ удален", "Ок");
                ProductsPanel.IsVisible = false;
                await ShowShopOrders(false);
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
    
    private async void OnPayShopOrder(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        try
        {
            var json = JsonSerializer.Serialize(_shopOrderID);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/shop/updateShopOrder", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успешно", "Заказ оплачен", "Ок");
                ProductsPanel.IsVisible = false;
                await ShowShopOrders(false);
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