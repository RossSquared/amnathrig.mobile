using IdentityModel.Client;
using IdentityModel.OidcClient;
using System.Collections.ObjectModel;
using System.Text.Json;
using MauiApp1.Models;
using BarcodeScanning;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    private readonly OidcClient _client;
    private string _currentAccessToken;
    public Command StartScanCommand { get; }

    // Observable collection to bind to the CollectionView
    public ObservableCollection<Asset> Assets { get; set; } = new ObservableCollection<Asset>();

    public MainPage(OidcClient client)
    {
        InitializeComponent();
        _client = client;

        StartScanCommand = new Command(StartScanning);

        // Binding the Assets collection to the view
        BindingContext = this;

        // Automatically log in and load assets
       
    }

    private async void StartScanning()
    {
        await Methods.AskForRequiredPermissionAsync();
        // Show the QR code scanner
        qrCodeScanner.IsVisible = true;
        qrCodeScanner.IsEnabled = true;
    }

    private async void OnBarcodeDetected(object sender, OnDetectionFinishedEventArg  e)
    {
        try
        {
            var qrcodeVal = "";
            if (e.BarcodeResults.Length == 0)
            {
                await DisplayAlert("Error", "No QR code detected.", "OK");
                return;
            }
            else
            {
                qrcodeVal = e.BarcodeResults[0].DisplayValue;
            }

            qrCodeScanner.IsVisible = false;

            // Send the QR code to the backend for processing
            await VerifyAndProcessTransfer(qrcodeVal);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        
    }

    private async Task VerifyAndProcessTransfer(string qrCodeData)
    {
        using(var client = new HttpClient())
        {
            client.SetBearerToken(_currentAccessToken);
            var response = await client.PostAsync("https://dev.amnathrig.app/assets/verify-transfer", new StringContent(qrCodeData));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TransferResponse>(content);

                if (result.Message != null)
                {
                    await DisplayAlert("Info", result.Message, "OK");
                }
                else
                {
                    await Navigation.PushAsync(new TransferDetailsPage(result.Asset, result.OwnerUsername));
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to verify transfer.", "OK");
            }
        }       
    }



    private async void LoadAssets()
    {
        var result = await _client.LoginAsync();
        if (result.IsError)
        {
            await DisplayAlert("Login Failed", result.Error, "OK");
            return;
        }

        _currentAccessToken = result.AccessToken;
        Preferences.Set("accessToken", _currentAccessToken);

        // Fetch assets
        await FetchAssets();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var result = await _client.LogoutAsync();
        LoadAssets();
    }

     protected override void OnAppearing()
    {
        base.OnAppearing();
        // Re-fetch assets every time the MainPage appears
        if (string.IsNullOrEmpty(_currentAccessToken))
        {
            LoadAssets();
        }
        else
        {
            FetchAssets();
        }   
    }

    private async Task FetchAssets()
    {
        using (var client = new HttpClient())
        {
            client.SetBearerToken(_currentAccessToken);
            var response = await client.GetAsync("https://dev.amnathrig.app/assets");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var assets = JsonSerializer.Deserialize<List<Asset>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Assets.Clear();
                foreach (var asset in assets)
                {
                    Assets.Add(asset);
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load assets.", "OK");
            }
        }
    }

    // Command to handle creating a new asset
    public Command CreateAssetCommand => new Command(async () =>
    {
        // Navigate to CreateAssetPage without an assetId for creating a new asset
        await Navigation.PushAsync(new CreateAssetPage(null));
    });

    // Command to handle editing an asset
    public Command EditAssetCommand => new Command<int>(async (assetId) =>
    {
        // Navigate to CreateAssetPage with the selected assetId for editing
        await Navigation.PushAsync(new CreateAssetPage(assetId));
    });

    // Method to handle deletion of the asset
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var assetId = (int)button.CommandParameter;

        var confirmed = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this asset?", "Yes", "No");
        if (confirmed)
        {
            using (var client = new HttpClient())
            {
                client.SetBearerToken(_currentAccessToken);
                var response = await client.DeleteAsync($"https://dev.amnathrig.app/assets/{assetId}");
                if (response.IsSuccessStatusCode)
                {
                    // Remove the deleted asset from the collection
                    var assetToRemove = Assets.FirstOrDefault(a => a.Id == assetId);
                    if (assetToRemove != null)
                    {
                        Assets.Remove(assetToRemove);
                    }

                    await DisplayAlert("Success", "Asset deleted successfully.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to delete asset.", "OK");
                }
            }
        }
    }
}
