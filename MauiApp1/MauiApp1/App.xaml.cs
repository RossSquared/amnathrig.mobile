using IdentityModel.OidcClient;

namespace MauiApp1;

public partial class App : Application
{
    public App(OidcClient oidcClient)
    {
        InitializeComponent();

        MainPage = new NavigationPage(new MainPage(oidcClient));
    }
}
