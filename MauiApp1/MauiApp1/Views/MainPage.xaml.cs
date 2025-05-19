using MauiApp1.Storage;
using MauiApp1.Views;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public MainPage(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }

        private void ActivityIndicatorRunning()
        {
            ButtonForLogin.IsEnabled = false;
            activityIndicator.IsVisible = true;
            activityIndicator.IsRunning = true;
            overlay.IsVisible = true;
        }
        private void ActivivtyIndicatorStopping()
        {
            ButtonForLogin.IsEnabled = true;
            activityIndicator.IsVisible = false;
            activityIndicator.IsRunning = false;
            overlay.IsVisible = false;
        }
        private async void ButtonClick(object sender, EventArgs e)
        {
            ActivityIndicatorRunning();
            if (string.IsNullOrEmpty(loginName.Text) && string.IsNullOrEmpty(passwordName.Text))
            {
                await DisplayAlert("Ошибка!", "Введите данные!", "Ок");
                ActivivtyIndicatorStopping();
                return;
            }
            else if (string.IsNullOrEmpty(loginName.Text))
            {
                await DisplayAlert("Ошибка!", "Введите логин!", "Ок");
                ActivivtyIndicatorStopping();
                return;
            }
            else if (string.IsNullOrEmpty(passwordName.Text))
            {
                await DisplayAlert("Ошибка!", "Введите пароль!", "Ок");
                ActivivtyIndicatorStopping();
                return;
            }

            var loginModel = new 
            { 
                Username = loginName.Text, 
                Password = passwordName.Text, 
                Firstname = "",
                Lastname = "",
                Middlename = ""
            };

            var json = JsonSerializer.Serialize(loginModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("/api/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<Users>(responseJson, new JsonSerializerOptions { WriteIndented = true });
                    UserManager.CurrentLogin = user.Username;
                    UserManager.UserId = user.UserId;
                    UserManager.UserType = user.UserType;
                    UserManager.Firstname = user.Firstname;
                    UserManager.Middlename = user.Middlename;
                    UserManager.Lastname = user.Lastname;

                    Application.Current.MainPage = new AppShell(_httpClient);
                }
                else
                {
                    await DisplayAlert("Ошибка!", "Неправильный ввод данных", "Ок");
                }
            }
            catch
            {
                await DisplayAlert("Ошибка!", "Проблемы на стороне сервера", "Ок");
            }
            finally
            {
                ActivivtyIndicatorStopping();
            }
        }
        private async void Registration(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new RegistrationPage(_httpClient));
        }
        private async void ForgotPassword(object sender, EventArgs e)
        {

        }
    }
}
