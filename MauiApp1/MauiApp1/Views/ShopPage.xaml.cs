using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using MauiApp1.Models;
using MauiApp1.Storage;
using MauiApp1.Views;

namespace MauiApp1;

public partial class ShopPage : ContentPage
{
    public ICommand RefreshCommand { get; }
    private readonly HttpClient _httpClient;
    private List<ShopItem> _shopItem = new();
    private bool _isBasket = false;
    public ShopPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
        RefreshCommand = new Command(async () => await ShowItemsForProducts(true));
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (UserManager.UserType != 1)
        {
            ForMasterFieldShop.IsVisible = true;
        }
        await ShowItemsForProducts(false);
    }

    private async Task ShowItemsForProducts(bool isRefresh = false)
    {
        ItemCount.Text = BasketItems.Count.ToString();
        ItemPrice.Text = BasketItems.Price.ToString();

        try
        {
            var response = await _httpClient.GetAsync("/api/shop/products");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ShopItem>>(json, new JsonSerializerOptions { WriteIndented = true });

                foreach (var product in products)
                {
                    foreach(var item in BasketItems.ItemsID)
                    {
                        if (product.ProductID == item)
                        {
                            product.IsPlus = false;
                        }
                    }
                }
                _shopItem = products;
                ShopItems.ItemsSource = products;
            }
            else
            {
                await DisplayAlert("Ошибка!", "Ошибка загрузки товаров", "Ок");
                return;
            }

            if (isRefresh)
            {
                ShopRefreshView.IsRefreshing = false;
            }
        }
        catch
        {
            await DisplayAlert("Ошибка!", "Проблемы на стороне сервера", "Ок");
        }
    }

    private void ActivityIndicatorRunning()
    {
        ButtonForAddShopOrder.IsEnabled = false;
        AddNewProduct.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivityIndicatorStopping()
    {
        ButtonForAddShopOrder.IsEnabled = true;
        AddNewProduct.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }
    private void PlusClicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var item = button.BindingContext as ShopItem;

        if (item == null) 
        {
            return;
        }

        if (item.IsPlus)
        {
            item.IsPlus = false;
            button.Source = "minus_icon.png";

            BasketItems.Price += item.Price;
            BasketItems.Count += 1;

            if (BasketItems.ItemsID != null)
            {
                var list = BasketItems.ItemsID.ToList();
                list.Add(item.ProductID);
                BasketItems.ItemsID = list.ToArray();
            }
        }
        else
        {
            item.IsPlus = true;
            button.Source = "plus_icon.png";

            BasketItems.Price -= item.Price;
            BasketItems.Count -= 1;

            if (BasketItems.ItemsID != null)
            {
                var list = BasketItems.ItemsID.ToList();
                list.Remove(item.ProductID);
                BasketItems.ItemsID = list.ToArray();
            }
        }

        foreach (var product in _shopItem)
        {
            if (product.ProductID == item.ProductID)
            {
                product.IsPlus = item.IsPlus;
            }
        }

        ItemCount.Text = BasketItems.Count.ToString();
        ItemPrice.Text = BasketItems.Price.ToString();
    }

    private async void ButtonToAddNewProduct(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddProductPage(_httpClient));
    }

    private async void ButtonToProduct(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ShopItem selectedProduct)
            await Navigation.PushAsync(new ProductPage(selectedProduct, _httpClient));

        ((CollectionView)sender).SelectedItem = null;
    }

    private async void ButtonToBasket(object sender, EventArgs e)
    {
        if (!_isBasket)
        {

            var filteredList = _shopItem.Where(n => n.IsPlus == false);
            ShopItems.ItemsSource = filteredList;
        }
        else
        {
            ShopItems.ItemsSource = _shopItem;
        }

        _isBasket = !_isBasket;
    } 

    private async void AddShopOrder(object sender, EventArgs e)
    {
        ActivityIndicatorRunning();
        var newShopOrder = new ShopOrders
        {
            UserID = UserManager.UserId,
            Price = BasketItems.Price
        };

        var json = JsonSerializer.Serialize(newShopOrder);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync("/api/shop/addShopOrder", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseContent = JsonSerializer.Deserialize<ShopOrders>(responseJson, new JsonSerializerOptions { WriteIndented = true });

                foreach (var item in _shopItem)
                {
                    if(item.IsPlus == false)
                    {
                        item.IsOrdered = true;
                        item.ShopOrderID = responseContent.ShopOrderID;

                        try
                        {
                            json = JsonSerializer.Serialize(item);
                            content = new StringContent(json, Encoding.UTF8, "application/json");

                            response = await _httpClient.PutAsync($"/api/shop/updateProduct/{item.ProductID}", content);

                        }
                        catch
                        {
                            await DisplayAlert("Ошибка!", "Проблемы на стороне сервера", "Ок");
                        }
                    }
                }

                await DisplayAlert("Успешно!", "Заказ отправлен", "Ок");
            }
        }
        catch
        {
            await DisplayAlert("Ошибка!", "Проблемы на стороне сервера", "Ок");
        }
        finally
        {
            ActivityIndicatorStopping();
        }

        BasketItems.Count = 0;
        BasketItems.Price = 0;
        BasketItems.ItemsID = new int[0];
        await ShowItemsForProducts(false);
    }

    private async void ShopOrdersHistory(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ShopOrdersHistory(_httpClient));
    }
}