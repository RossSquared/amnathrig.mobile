using IdentityModel.Client;
using IdentityModel.OidcClient;
using System.Text.Json;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    private readonly OidcClient _client;
    private string? _currentAccessToken;

    public MainPage(OidcClient client)
    {
        InitializeComponent();
        _client = client;
    }

    // Automatically called when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AutoLoginAndFetchAssets();
    }

    // Automatically login and fetch assets
    private async Task AutoLoginAndFetchAssets()
    {
        MessageLabel.Text = "Attempting to log in...";

        // Log in to get the access token
        var loginResult = await _client.LoginAsync();

        if (loginResult.IsError)
        {
            MessageLabel.Text = $"Login Error: {loginResult.Error}";
            return;
        }

        _currentAccessToken = loginResult.AccessToken;
        Preferences.Set("accessToken", _currentAccessToken);


        // Fetch the assets after login
        await FetchAssets();
    }

    // Fetch assets from the API using the access token
    private async Task FetchAssets()
    {
        MessageLabel.Text = "Fetching assets...";

        if (!string.IsNullOrEmpty(_currentAccessToken))
        {
            var httpClient = new HttpClient();
            httpClient.SetBearerToken(_currentAccessToken);

            var response = await httpClient.GetAsync("https://dev.amnathrig.app/assets");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var assets = JsonDocument.Parse(content).RootElement.EnumerateArray();

                // Clear previous content
                AssetsStackLayout.Children.Clear();

                // Loop through each asset and create UI elements dynamically
                foreach (var asset in assets)
                {
                    // Create labels for asset details
                    var assetNameLabel = new Label
                    {
                        Text = $"Asset Name: {asset.GetProperty("name").GetString()}",
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        TextColor = Colors.Black
                    };

                    var assetDescriptionLabel = new Label
                    {
                        Text = $"Description: {asset.GetProperty("description").GetString()}",
                        FontSize = 14,
                        TextColor = Colors.Gray
                    };

                    var assetTypeLabel = new Label
                    {
                        Text = $"Type: {asset.GetProperty("assetType").GetString()}",
                        FontSize = 14,
                        TextColor = Colors.Gray
                    };

                    // Add asset labels to the layout
                    AssetsStackLayout.Children.Add(assetNameLabel);
                    AssetsStackLayout.Children.Add(assetDescriptionLabel);
                    AssetsStackLayout.Children.Add(assetTypeLabel);

                    // If the asset has properties, create labels for each property
                    if (asset.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var property in properties.EnumerateArray())
                        {
                            var propertyLabel = new Label
                            {
                                Text = $"{property.GetProperty("name").GetString()}: {property.GetProperty("value").GetString()}",
                                FontSize = 14,
                                TextColor = Colors.DarkGray
                            };
                            AssetsStackLayout.Children.Add(propertyLabel);
                        }
                    }

                    // Add spacing between assets
                    AssetsStackLayout.Children.Add(new BoxView { HeightRequest = 20, BackgroundColor = Colors.Transparent });
                }

                MessageLabel.Text = string.Empty; // Clear message after assets are displayed
            }
            else
            {
                MessageLabel.Text = $"Error fetching assets: {response.ReasonPhrase}";
            }
        }
        else
        {
            MessageLabel.Text = "No access token available to fetch assets.";
        }
    }

    // Logout functionality
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await LogoutAndLoginAgain();
    }

    private async Task LogoutAndLoginAgain()
    {
        MessageLabel.Text = "Logging out...";

        var result = await _client.LogoutAsync();

        if (result.IsError)
        {
            MessageLabel.Text = $"Logout Error: {result.Error}";
            return;
        }

        MessageLabel.Text = "Logged out. Attempting to login again...";
        //await AutoLoginAndFetchAssets();
    }

    private async void OnCreateAssetClicked(object sender, EventArgs e)
    {
        // Navigate to CreateAssetPage
        await Navigation.PushAsync(new CreateAssetPage());
    }
}
