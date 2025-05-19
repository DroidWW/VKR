using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using MauiApp1.Models;
using MauiApp1.Storage;
using MauiApp1.Views;
using Microsoft.Maui.Controls;


namespace MauiApp1;

public partial class NewsPage : ContentPage
{
    public ICommand RefreshCommand { get;}
    private readonly HttpClient _httpClient;
    private List<Reports> _allReports = new();
	public NewsPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
        RefreshCommand = new Command(async () => await ShowItemsForNews(true));
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (UserManager.UserType != 1)
        {
            ForMasterField.IsVisible = true;
        }

        await ShowItemsForNews(false);
    }

    private async Task ShowItemsForNews(bool isRefresh = false)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/reports/reports");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var reports = JsonSerializer.Deserialize<List<Reports>>(json, new JsonSerializerOptions { WriteIndented = true });
                _allReports = reports;
                CollectionNews.ItemsSource = reports;
            }
            else
            {
                Debug.WriteLine($"Ошибка загрузки новостей");
                return;
            }
            if (isRefresh)
            {
                NewsRefreshView.IsRefreshing = false;
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
    }

    private async void ButtonToAddNewsPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddReportPage(_httpClient));
    }

    private async void OnItemTapped(object sender, EventArgs e)
    {
        var grid = sender as Grid;
        var item = grid?.BindingContext as Reports;

        if (item != null)
        {
            await Navigation.PushAsync(new ReportPage(item, _httpClient));
        }
    }

    private async void OnLikeIconTapped(object sender, EventArgs e)
    {
        var tappedElement = sender as Image;
        var item = tappedElement?.Parent.BindingContext as Reports;

        if(item != null)
        {

            var likesDislikesModel = new LikesDislikes
            {
                ReportID = item.ReportID,
                UserID = UserManager.UserId

            };

            var json = JsonSerializer.Serialize(likesDislikesModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/reports/checker", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseContent = JsonSerializer.Deserialize<LikesDislikes>(responseJson, new JsonSerializerOptions { WriteIndented = true });
                    if (!responseContent.IsLiked)
                    {
                        if (responseContent.IsDisliked)
                        {
                            responseContent.IsDisliked = false;
                            item.DislikeCount -= 1;
                        }

                        responseContent.IsLiked = true;
                        item.LikeCount += 1;
                    }
                    else
                    {
                        responseContent.IsLiked = false;
                        item.LikeCount -= 1;
                    }

                    var json2 = JsonSerializer.Serialize(responseContent);
                    var content2 = new StringContent(json2, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("/api/reports/update", content2);

                }
            }
            catch
            {
                await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
            }
        }
    }
    private async void OnDisLikeIconTapped(object sender, EventArgs e)
    {
        var tappedElement = sender as Image;
        var item = tappedElement?.Parent.BindingContext as Reports;

        if (item != null)
        {

            var likesDislikesModel = new LikesDislikes
            {
                ReportID = item.ReportID,
                UserID = UserManager.UserId

            };

            var json = JsonSerializer.Serialize(likesDislikesModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/reports/checker", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseContent = JsonSerializer.Deserialize<LikesDislikes>(responseJson, new JsonSerializerOptions { WriteIndented = true });
                    
                    if (!responseContent.IsDisliked)
                    {
                        if (responseContent.IsLiked)
                        {
                            responseContent.IsLiked = false;
                            item.LikeCount -= 1;
                        }

                        responseContent.IsDisliked = true;
                        item.DislikeCount += 1;
                    }
                    else
                    {
                        responseContent.IsDisliked = false;
                        item.DislikeCount -= 1;
                    }

                    var json2 = JsonSerializer.Serialize(responseContent);
                    var content2 = new StringContent(json2, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("/api/reports/update", content2);
                }
            }
            catch
            {
                await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
            }
        }
    }

    private async void OnCommentsIconTapped(object sender, EventArgs e)
    {
        var tappedElement = sender as Image;
        var item = tappedElement?.Parent.BindingContext as Reports;

        if (item != null)
        {
            await Navigation.PushAsync(new CommentsPage(item, _httpClient));
        }
    }

    private void OnSearchBar(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            CollectionNews.ItemsSource = _allReports;
            return;
        }

        var filteredList = _allReports.Where(n => n.Name.ToLower().Contains(searchText) 
                || n.Description.ToLower().Contains(searchText)).ToList();
        CollectionNews.ItemsSource = filteredList;
    }
}