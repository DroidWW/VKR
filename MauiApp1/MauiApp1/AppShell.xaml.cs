using MauiApp1.Storage;
using MauiApp1.Views;

namespace MauiApp1;

public partial class AppShell : Shell
{
    private readonly HttpClient _httpClient;

    public AppShell(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
        Routing.RegisterRoute("NewsPage", typeof(NewsPage));
		Routing.RegisterRoute("MessagesPage", typeof(MessagesPage));
		Routing.RegisterRoute("OrderPage", typeof(OrderPage));
		Routing.RegisterRoute("ShopPage", typeof(ShopPage));
		Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
		Routing.RegisterRoute("AddProductPage", typeof(AddProductPage));
		Routing.RegisterRoute("AddReportPage", typeof(AddReportPage));
		Routing.RegisterRoute("ChatPage", typeof(ChatPage));
		Routing.RegisterRoute("ReportPage", typeof(ReportPage));
		Routing.RegisterRoute("ProductPage", typeof(ProductPage));
		Routing.RegisterRoute("MasterOrderPage", typeof(ProductPage));
		Routing.RegisterRoute("EditOrderPage", typeof(EditOrderPage));
		Routing.RegisterRoute("AddMasterPage", typeof(AddMasterPage));
		Routing.RegisterRoute("CommentsPage", typeof(CommentsPage));

		var orderTabCopy = OrderTab;
		MainTabBar.Items.Remove(orderTabCopy);

        var newShellContent = new ShellContent
		{
			Icon = "order_icon.png",
			ContentTemplate = new DataTemplate(UserManager.UserType == 1 ? typeof(OrderPage) : typeof(MasterOrderPage))
        };

		MainTabBar.Items.Insert(2, newShellContent);
	}
}