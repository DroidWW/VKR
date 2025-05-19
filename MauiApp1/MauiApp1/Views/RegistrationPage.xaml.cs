using System.Text.Json;
using System.Text;
using System.Diagnostics;

namespace MauiApp1.Views;

public partial class RegistrationPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public RegistrationPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
    }

    private void ActivityIndicatorRunning()
    {
        ButtonForContinue.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivivtyIndicatorStopping()
    {
        ButtonForContinue.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

    public async void ContinuePage(object sender, EventArgs e)
	{
        ActivityIndicatorRunning();
        if (string.IsNullOrWhiteSpace(loginName.Text) || string.IsNullOrWhiteSpace(passwordName.Text)
            || string.IsNullOrWhiteSpace(passwordNameAgain.Text) || string.IsNullOrWhiteSpace(secondName.Text)
            || string.IsNullOrWhiteSpace(firstName.Text) || string.IsNullOrWhiteSpace(thirdName.Text))
        {
            await DisplayAlert("������!", "�� ��� ���� ���������!", "��");
            ActivivtyIndicatorStopping();
            return;
        }

        if (passwordName.Text != passwordNameAgain.Text)
        {
            await DisplayAlert("������!", "������ �� ���������!", "��");
            ActivivtyIndicatorStopping();
            return;
        }

        if (!agreementSwitch.IsToggled) 
        {
            await DisplayAlert("������!", "�� �� ����������� � ��������� � ���������!", "��");
            ActivivtyIndicatorStopping();
            return;
        }

        var registerModel = new { 
            Username = loginName.Text, 
            Password = passwordName.Text,
            Firstname = firstName.Text,
            Lastname = thirdName.Text,
            Middlename = secondName.Text,
            UserType = 1
        };

        var json = JsonSerializer.Serialize(registerModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("/api/registration/registration", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("�����", "����������� ������ �������!", "��");
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlert("������", "������������ � ����� ������ ��� ����������!", "��");
            }
        }
        catch
        {
            await DisplayAlert("������", "�������� �� ������� �������", "��");
        }
        finally
        {
            ActivivtyIndicatorStopping();
        }

    }
    public async void BackToMainPage(object sender, EventArgs e)
    {
		await Navigation.PopModalAsync();
    }
}