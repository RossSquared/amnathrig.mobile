namespace amnathrig.mobile
{
    public partial class MainPage : ContentPage
    {
        private readonly OidcAuthenticationService _authService;

        public MainPage()
        {
            InitializeComponent();
            _authService = new OidcAuthenticationService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                var loginResult = await _authService.LoginAsync();
                // Handle successful login, navigate to a new page, etc.
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Failed", ex.Message, "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                await _authService.LogoutAsync();
                // Handle successful logout
            }
            catch (Exception ex)
            {
                await DisplayAlert("Logout Failed", ex.Message, "OK");
            }
        }
    }


}
