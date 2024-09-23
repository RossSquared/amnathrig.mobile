using IdentityModel.Client;
using MauiApp1.Models;
using System.Net.Http.Json;
using System.Windows.Input;

namespace MauiApp1
{
    public partial class TransferDetailsPage : ContentPage
    {
        public Asset Asset { get; set; }
        public string OwnerUsername { get; set; }

        public string _currentAccessToken;
        public ICommand RequestTransferCommand { get; }

        public TransferDetailsPage(Asset asset, string ownerUsername)
        {
            InitializeComponent();

            Asset = asset;
            OwnerUsername = ownerUsername;

            // Command for requesting transfer
            RequestTransferCommand = new Command(async () => await RequestTransfer());

            // Set the BindingContext to bind data to the UI
            BindingContext = this;
        }

        private async Task RequestTransfer()
        {
            _currentAccessToken = Preferences.Get("accessToken", string.Empty);
            var client = new HttpClient();
            client.SetBearerToken(_currentAccessToken);

            var transferRequest = new
            {
                AssetId = Asset.Id,
                FromUserId = Asset.UserId
            };

            var response = await client.PostAsJsonAsync("https://dev.amnathrig.app/assets/initiate-transfer", transferRequest);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Transfer initiated successfully.", "OK");
                await Navigation.PopAsync(); // Go back to the previous page
            }
            else
            {
                await DisplayAlert("Error", "Failed to initiate transfer.", "OK");
            }
        }
    }
}
