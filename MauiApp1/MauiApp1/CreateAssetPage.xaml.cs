using IdentityModel.Client;
using IdentityModel.OidcClient;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1
{
    public partial class CreateAssetPage : ContentPage
    {
        private string _currentAccessToken;
        private int? _assetId; // Used to determine if we're editing an existing asset
        private string _customAssetType;
        private Asset _asset;
        public ObservableCollection<AssetProperty> Properties { get; set; } = new ObservableCollection<AssetProperty>();

        public List<string> PredefinedTypes { get; set; } = new List<string>
        {
            "Vehicle",
            "Appliance",
            "Real Estate Property",
            "Furniture",
            "Electronics"
        };

        public CreateAssetPage(int? assetId = null)
        {
            InitializeComponent();
           
            _assetId = assetId;
            _asset = new Asset();

            BindingContext = this;

            // Set the title and button text based on whether we're editing or creating
            if (_assetId.HasValue)
            {
                Title = "Edit Asset";
                CreateButton.Text = "Update Asset";
                LoadAssetDetails(_assetId.Value); // Load existing asset details if we're editing
            }
            else
            {
                Title = "Create New Asset";
                CreateButton.Text = "Create Asset";
            }

           
        }

        private async void LoadAssetDetails(int assetId)
        {
            using (var client = new HttpClient())
            {
                _currentAccessToken = Preferences.Get("accessToken", string.Empty);
                client.SetBearerToken(_currentAccessToken);

                var response = await client.GetAsync($"https://dev.amnathrig.app/assets/{assetId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var asset = JsonSerializer.Deserialize<Asset>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _asset = asset;

                    AssetNameEntry.Text = asset.Name;
                    AssetDescriptionEntry.Text = asset.Description;

                    // Select the predefined type if available
                    if (PredefinedTypes.Contains(_asset.AssetType))
                    {
                        PredefinedTypesPicker.SelectedItem = _asset.AssetType;
                    }
                    else
                    {
                        CustomAssetTypeEntry.Text = _asset.AssetType;
                    }

                   

                    // Load properties into the observable collection
                    Properties.Clear();
                    foreach (var property in asset.Properties)
                    {
                        Properties.Add(property);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load asset details.", "OK");
                }
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _currentAccessToken = Preferences.Get("accessToken", string.Empty);
            var newAsset = new Asset
            {
                Name = AssetNameEntry.Text,
                Description = AssetDescriptionEntry.Text,
                Properties = Properties.ToList()
            };

            if (!string.IsNullOrWhiteSpace(CustomAssetTypeEntry.Text))
            {
                newAsset.AssetType = CustomAssetTypeEntry.Text;
            }
            else
            {
                newAsset.AssetType = PredefinedTypesPicker.SelectedItem?.ToString();
            }


            using (var client = new HttpClient())
            {
                client.SetBearerToken(_currentAccessToken);

                HttpResponseMessage response;
                if (_assetId.HasValue)
                {
                    // Update existing asset
                    response = await client.PutAsJsonAsync($"https://dev.amnathrig.app/assets/{_assetId}", newAsset);
                }
                else
                {
                    // Create new asset
                    response = await client.PostAsJsonAsync("https://dev.amnathrig.app/assets", newAsset);
                }

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", _assetId.HasValue ? "Asset updated successfully." : "Asset created successfully.", "OK");
                    await Navigation.PopAsync(); // Go back to the previous page
                }
                else
                {
                    await DisplayAlert("Error", "Failed to save the asset.", "OK");
                }
            }
        }

        private void OnCustomAssetTypeChanged(object sender, TextChangedEventArgs e)
        {
            _customAssetType = e.NewTextValue;
        }

        private void OnAddPropertyClicked(object sender, EventArgs e)
        {
            Properties.Add(new AssetProperty { Name = "", Value = "" });
        }

        private void OnRemovePropertyClicked(object sender, EventArgs e)
        {
            var property = (AssetProperty)((Button)sender).BindingContext;
            Properties.Remove(property);
        }
    }
}
