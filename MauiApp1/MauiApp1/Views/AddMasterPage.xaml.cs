using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace MauiApp1.Views;

public partial class AddMasterPage : ContentPage
{
    private readonly HttpClient _httpClient;
	public AddMasterPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
	}

    private void ActivityIndicatorRunning()
    {
        ButtonForAddMaster.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivityIndicatorStopping()
    {
        ButtonForAddMaster.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

	private async void AddMasterButton(object sender, EventArgs e)
	{
        ActivityIndicatorRunning();

        if (string.IsNullOrWhiteSpace(loginName.Text) || string.IsNullOrWhiteSpace(passwordName.Text)
            || string.IsNullOrWhiteSpace(passwordNameAgain.Text) || string.IsNullOrWhiteSpace(secondName.Text)
            || string.IsNullOrWhiteSpace(firstName.Text) || string.IsNullOrWhiteSpace(thirdName.Text))
        {
            await DisplayAlert("Ошибка!", "Не все поля заполнены!", "Ок");
            ActivityIndicatorStopping();
            return;
        }

        if (passwordName.Text != passwordNameAgain.Text)
        {
            await DisplayAlert("Ошибка!", "Пароли не совпадают!", "Ок");
            ActivityIndicatorStopping();
            return;
        }

        var registerModel = new
        {
            Username = loginName.Text,
            Password = passwordName.Text,
            Firstname = firstName.Text,
            Lastname = thirdName.Text,
            Middlename = secondName.Text,
            UserType = 3
        };

        var json = JsonSerializer.Serialize(registerModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("/api/registration/registration", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успех", $"Мастер добавлен!", "Ок");
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Пользователь с таким именем уже существует!", "Ок");
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