using System.Text.Json;
using System.Text;
using MauiApp1.Storage;
using System.Diagnostics;
using MauiApp1.Views;
using System.Windows.Input;

namespace MauiApp1;

public partial class MessagesPage : ContentPage
{
    public ICommand RefreshCommand { get; }
    private readonly HttpClient _httpClient;
    public MessagesPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
        RefreshCommand = new Command(async () => await ShowChats(true));
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ShowChats(false);
    }

    private async Task ShowChats(bool isRefresh = false)
    {

        var json = JsonSerializer.Serialize(UserManager.UserId);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("/api/messages/chats", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var chats = JsonSerializer.Deserialize<List<Chat>>(responseJson, new JsonSerializerOptions { WriteIndented = true });

                Debug.WriteLine("test: " + responseJson);
                if (StatusPicker.SelectedIndex > 0 && chats != null)
                {
                    int selectedStatusId = StatusPicker.SelectedIndex;
                    chats = chats.Where(c => c.StatusID == selectedStatusId).ToList();
                }

                CollectionMessages.ItemsSource = chats;
            }
            else
            {
                await DisplayAlert("Ошибка!", "Не получено никаких данных", "Ок");
            }
            if (isRefresh)
            {
                ChatRefreshView.IsRefreshing = false;
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
    }

    private async void ButtonToChat(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Chat selectedChat)
            await Navigation.PushAsync(new ChatPage(selectedChat, _httpClient));

        ((CollectionView)sender).SelectedItem = null;
    }
    private async void OnStatusPicker(object sender, EventArgs e)
    {
        await ShowChats(false);
    }
}