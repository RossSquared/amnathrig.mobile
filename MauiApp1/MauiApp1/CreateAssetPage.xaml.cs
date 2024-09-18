using IdentityModel.OidcClient;
using Microsoft.Maui.Storage; // For Preferences
using System.Net.Http.Headers;
using System.Text.Json;

namespace MauiApp1;

public partial class CreateAssetPage : ContentPage
{
    private readonly string _apiBaseUrl = "https://dev.amnathrig.app";
    private List<PropertyEntry> _properties = new List<PropertyEntry>(); // To hold the properties dynamically added

    public CreateAssetPage()
    {
        InitializeComponent();
   
    }

    // Handle adding a new property input set
    private void OnAddPropertyClicked(object sender, EventArgs e)
    {
        var propertyEntry = new PropertyEntry(); // A helper class to hold property data
        _properties.Add(propertyEntry);

        var nameEntry = new Entry { Placeholder = "Property Name" };
        var valueEntry = new Entry { Placeholder = "Property Value" };

        propertyEntry.NameEntry = nameEntry;
        propertyEntry.ValueEntry = valueEntry;

        PropertiesStackLayout.Children.Add(nameEntry);
        PropertiesStackLayout.Children.Add(valueEntry);
    }

    private async void OnCreateAssetClicked(object sender, EventArgs e)
    {
        string token = Preferences.Get("accessToken", string.Empty); // Retrieve token from Preferences

        if (string.IsNullOrEmpty(token))
        {
            StatusLabel.Text = "You are not authenticated.";
            return;
        }

        // Create the properties list
        var propertiesList = _properties
            .Where(p => !string.IsNullOrWhiteSpace(p.NameEntry.Text) && !string.IsNullOrWhiteSpace(p.ValueEntry.Text))
            .Select(p => new { Name = p.NameEntry.Text, Value = p.ValueEntry.Text })
            .ToList();

        var newAsset = new
        {
            Name = AssetNameEntry.Text,
            Description = AssetDescriptionEditor.Text,
            AssetType = !string.IsNullOrEmpty(CustomAssetTypeEntry.Text)
                ? CustomAssetTypeEntry.Text
                : AssetTypePicker.SelectedItem?.ToString(),
            Properties = propertiesList // Attach the properties
        };



        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(newAsset), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiBaseUrl}/assets", content);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Asset created successfully.", "OK");
                await Navigation.PopAsync(); // Go back to the previous page after successful creation
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                StatusLabel.Text = $"Error: {errorContent}";
            }
        }
    }
}

// Helper class to hold the property data (Name/Value)
public class PropertyEntry
{
    public Entry NameEntry { get; set; }
    public Entry ValueEntry { get; set; }
}
