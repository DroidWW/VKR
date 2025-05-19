using MauiApp1.Models;
using MauiApp1.Storage;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

namespace MauiApp1.Views;

public partial class ChatPage : ContentPage
{
	private readonly HttpClient _httpClient;
    private bool _isMinInfo = true;
    private readonly Chat _chat;
	private HubConnection _hubConnection;
	private ObservableCollection<MessageModel> _messages;
	public ChatPage(Chat chat, HttpClient httpClient) : base()
	{
		InitializeComponent();
		_httpClient = httpClient;
        _chat = chat;
		BindingContext = chat;

		_messages = new ObservableCollection<MessageModel>();
        MessagesList.ItemsSource = _messages;

        InitializeSignalR();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        GetFields();
        GetStatus();
        await ShowMessagesForChat();
    }
    
    private void GetFields()
    {
        if (UserManager.UserType != 1)
        {
            StatusName.IsVisible = false;
            ServicePicker.IsVisible = true;
            if (_chat.StatusID >= 2)
                ForPayFieldMaster.IsVisible = true;
        }
        else
        {
            if (_chat.StatusID >= 2)
                ForPayFieldUser.IsVisible = true;
        }
    }

    private void GetStatus()
    {
        if (_chat.StatusID == 1)
        {
            StatusName.Text = "В процессе работы";
            ServicePicker.Title = "В процессе работы";
        }
        else if (_chat.StatusID == 2)
        {
            StatusName.Text = "Завершен";
            ServicePicker.Title = "Завершен";
        }
        else
        {
            StatusName.Text = "Выдан";
            ServicePicker.Title = "Выдан";
        }
    }

    private async Task ShowMessagesForChat()
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/messages/{_chat.OrderID}");
            if (response.IsSuccessStatusCode) 
            {
                var json = await response.Content.ReadAsStringAsync();
                var messages = JsonSerializer.Deserialize<List<MessageModel>>(json);
                foreach (var msg in messages)
                {
                    _messages.Add(msg);
                }
            }
        }
        catch
        {
            await DisplayAlert("Ошибка", "Не удалось загрузить историю сообщений", "Ок");
        }
    }

    private async void InitializeSignalR()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:8080/chathub")
            .Build();

        _hubConnection.On<int, string>("ReceiveMessage", (senderId, message) =>
        {
            Debug.WriteLine($"Получено сообщение от {senderId}: {message}");
            var messageModel = new MessageModel
            {
                SenderID = senderId,
                OrderID = _chat.OrderID,
                SentAt = DateTime.UtcNow,
                MessageText = message
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _messages.Add(messageModel);
                MessagesList.ScrollTo(messageModel);
            });
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch
        {
            await DisplayAlert("Ошибка", "Не удалось подключиться к чату", "Ок");
        }
    }

    private async void SendMessageButton(object sender, EventArgs e)
    {
        var messageText = MessageEntry.Text;

        if (string.IsNullOrWhiteSpace(messageText))
        {
            await DisplayAlert("Ошибка", "Введите сообщение", "Ок");
            return;
        }
        
        var senderID = UserManager.UserId;
        var recipientID = _chat.MasterID;

        var messageModel = new MessageModel
        {
            SenderID = senderID,
            RecipientID = (int)recipientID,
            OrderID= _chat.OrderID,
            MessageText = messageText,
            SentAt= DateTime.UtcNow
        };

        try
        {
            await _hubConnection.InvokeAsync("SendMessage", senderID, recipientID, messageText);

            MessageEntry.Text = "";

            var json = JsonSerializer.Serialize(messageModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("/api/messages/addMessage", content);
        }
        catch
        {
            await DisplayAlert("Ошибка", "Не удалось отправить сообщение", "Ок");
        }
        finally
        {
            _messages.Add(messageModel);
        }
    }

    private async void OnMinInfoFieldTapped(object sender, EventArgs e)
    {
        if (ServicePicker.SelectedIndex + 1 > _chat.StatusID)
        {
            var answer = await DisplayAlert("Подтверждение", "Точно ли хотите поменять статус заказа?", "Да", "Нет");
            if (!answer)
            {
                ServicePicker.SelectedItem = default;
            }
            else
            {
                _chat.StatusID = ServicePicker.SelectedIndex + 1;

                var json = JsonSerializer.Serialize(_chat);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    var response = await _httpClient.PutAsync($"/api/repairOrders/updateStatus/{_chat.OrderID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        GetFields();
                        GetStatus();
                    }
                }
                catch
                {
                    await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
                }
            }
        }


        _isMinInfo = !_isMinInfo;

        MinInfoButton.IsVisible = _isMinInfo;
        MaxInfoButton.IsVisible = !_isMinInfo;

        MinInfoField.IsVisible = _isMinInfo;
        MaxInfoField.IsVisible = !_isMinInfo;

        MessageEntry.IsVisible = _isMinInfo;

        ChatField.IsVisible = _isMinInfo;
    }

    private async void PayOrder(object sender, EventArgs e)
    {

    } 
    private async void SetPrice(object sender, EventArgs e)
    {
        var answer = await DisplayAlert("Подтверждение", "Точно ли хотите установить такую цену?", "Да", "Нет");

        if (!answer)
        {
            PriceEntry.Text = _chat.Price.ToString();
        }
        else
        {
            int.TryParse(PriceEntry.Text, out int price);
            _chat.Price = price;

            var json = JsonSerializer.Serialize(_chat);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync($"/api/repairOrders/updateStatus/{_chat.OrderID}", content);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Удачно", "Цена установлена", "Ок");
                    GetFields();
                }
            }
            catch
            {
                await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
            }
        }
    }
}