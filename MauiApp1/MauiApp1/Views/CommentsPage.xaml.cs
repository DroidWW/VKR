using MauiApp1.Models;
using MauiApp1.Storage;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MauiApp1.Views;

public partial class CommentsPage : ContentPage
{
	private readonly HttpClient _httpClient;
	private readonly Reports report;
	public CommentsPage(Reports reports, HttpClient httpClient)
	{
		InitializeComponent();
		_httpClient = httpClient;
		report = reports;
		CollectionComments.ItemsSource = reports.ReportsList;
	}

	public async void AddNewComment(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(CommentEditor.Text))
		{
            await DisplayAlert("Ошибка!", "Введите комментарий!", "Ок");
            return;
        }

        var newComment = new ReportsComments
        {
            ReportID = report.ReportID,
            UserID = UserManager.UserId,
            Text = CommentEditor.Text,
			CreatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(newComment);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
		{
			var response = await _httpClient.PostAsync("/api/reports/addComment", content);
			Debug.WriteLine("test: " + response);
			CommentEditor.Text = "";
            report.ReportsList?.Add(newComment);
        }
		catch
		{
            await DisplayAlert("Ошибка", "Проблемы на стороне сервера", "Ок");
        }
	}
}