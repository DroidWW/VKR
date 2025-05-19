using MauiApp1.Storage;
using MauiApp1.Views;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MauiApp1;

public partial class ProfilePage : ContentPage
{
	private readonly HttpClient _httpClient;
    private byte[] _selectedImagesBytes = null;
    public ProfilePage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
    }

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		UploadProfileImage();
		FullName.Text = UserManager.Middlename + " " + UserManager.Firstname + " " + UserManager.Lastname;
		if (UserManager.UserType == 3) 
		{
            AddMasterButton.IsVisible = true;
			WebMasterButton.IsVisible = true;
        }
	}

    private void ActivityIndicatorRunning()
    {
        ButtonForAccept.IsEnabled = false;
        AddMasterButton.IsEnabled = false;
        WebMasterButton.IsEnabled = false;
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        overlay.IsVisible = true;
    }
    private void ActivivtyIndicatorStopping()
    {
        ButtonForAccept.IsEnabled = true;
        AddMasterButton.IsEnabled = true;
        WebMasterButton.IsEnabled = true;
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
        overlay.IsVisible = false;
    }

    private async void UploadProfileImage()
	{
        var json = JsonSerializer.Serialize(UserManager.UserId);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
		try
		{
			var response = await _httpClient.PostAsync("/api/profile/getImage", content);

			if (response.IsSuccessStatusCode)
			{
				UserManager.ProfileImage = await response.Content.ReadAsByteArrayAsync();
				ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(UserManager.ProfileImage));
			}
			else
			{
				await DisplayAlert("Ошибка!", "Изображение не загрузилось!", "Ок");
			}
		}
		catch
		{
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }

    }

	private async void LogOutButton(object sender, EventArgs e)
	{
		UserManager.CurrentLogin = string.Empty;
        UserManager.Firstname = string.Empty;
        UserManager.Middlename = string.Empty;
        UserManager.Lastname = string.Empty;
        UserManager.UserId = 0;
        UserManager.UserType = 0;
		UserManager.ProfileImage = null;
        Application.Current.MainPage = new MainPage(_httpClient);
    }
	private async void ButtonForChangesInfo(object sender, EventArgs e)
	{
		ActivityIndicatorRunning();

        if (string.IsNullOrWhiteSpace(NewLogin.Text) && string.IsNullOrWhiteSpace(NewPassword.Text)
			&& string.IsNullOrWhiteSpace(AccessPassword.Text) && string.IsNullOrWhiteSpace(SecondName.Text)
			&& string.IsNullOrWhiteSpace(FirstName.Text) && string.IsNullOrWhiteSpace(ThirdName.Text)
			&& _selectedImagesBytes == null)
		{
            await DisplayAlert("Ошибка!", "Введите данные для обновления!", "Ок");
			ActivivtyIndicatorStopping();
            return;
        }

		if (NewPassword.Text != AccessPassword.Text) 
		{
            await DisplayAlert("Ошибка!", "Пароли не совподают!", "Ок");
			ActivivtyIndicatorStopping();
            return;
        }

        var answer = await DisplayAlert("Подтверждение", "Точно ли хотите поменять данные?", "Да", "Нет");

		if (!answer)
		{
			ActivivtyIndicatorStopping();
            return;
		}

        var newUserInfo = new
		{
			Username = NewLogin.Text ?? "",
			Password = NewPassword.Text ?? "",
			Firstname = FirstName.Text ?? "",
			Lastname = ThirdName.Text ?? "",
			Middlename = SecondName.Text ?? "",
			ImageURL = $"profile_{UserManager.UserId}.jpg",
			Data = _selectedImagesBytes
		};

        var json = JsonSerializer.Serialize(newUserInfo);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		try
		{
			var response = await _httpClient.PutAsync($"/api/profile/profile/{UserManager.UserId}", content);

			if (response.IsSuccessStatusCode)
			{

				if (!string.IsNullOrWhiteSpace(NewLogin.Text))
					UserManager.CurrentLogin = NewLogin.Text;

				if (!string.IsNullOrWhiteSpace(FirstName.Text))
				{
					UserManager.Firstname = FirstName.Text;
					FullName.Text = UserManager.Middlename + " " + UserManager.Firstname + " " + UserManager.Lastname;
				}

				if (!string.IsNullOrWhiteSpace(SecondName.Text))
				{
					UserManager.Middlename = SecondName.Text;
					FullName.Text = UserManager.Middlename + " " + UserManager.Firstname + " " + UserManager.Lastname;
				}

				if (!string.IsNullOrWhiteSpace(ThirdName.Text))
				{
					UserManager.Lastname = ThirdName.Text;
					FullName.Text = UserManager.Middlename + " " + UserManager.Firstname + " " + UserManager.Lastname;
				}

				if (UserManager.ProfileImage != _selectedImagesBytes)
				{
					UserManager.ProfileImage = _selectedImagesBytes;
					_selectedImagesBytes = null;
					ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(UserManager.ProfileImage));
				}

				NewLogin.Text = "";
				NewPassword.Text = "";
				AccessPassword.Text = "";
				FirstName.Text = "";
				SecondName.Text = "";
				ThirdName.Text = "";

				await DisplayAlert("Успех", "Данные успешно обновлены", "Ок");
			}
			else
			{
				await DisplayAlert("Ошибка", "Не удалось обновить данные", "Ок");
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

	private async void OnUpdateImageClicked(object sender, EventArgs e) 
	{
        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo == null)
            return;

        using var sourceStream = await photo.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream);

        _selectedImagesBytes = memoryStream.ToArray();
        ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(_selectedImagesBytes));
    }
	
	private async void ButtonForAddMaster(object sender, EventArgs e)
	{
        await Navigation.PushAsync(new AddMasterPage(_httpClient));
    }
	private async void ButtonToWeb(object sender, EventArgs e)
	{
		await Launcher.Default.OpenAsync("http://10.0.2.2:8080");
	}
}