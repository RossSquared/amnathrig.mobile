using IdentityModel.Client;
using MauiApp1.Models;
using System.Net.Http.Json;
using System.Text.Json;
using ZXing.Net.Maui;

namespace MauiApp1;

public partial class ScanPage : ContentPage
{

  
    private string _currentAccessToken;
    private bool isProcessingScan = false;
    public ScanPage()
	{

		InitializeComponent();

        qrCodeScanner.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    private void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
    {
        try
        {
            if (isProcessingScan)
            {
                return;
            }

            isProcessingScan = true;
            qrCodeScanner.BarcodesDetected -= OnBarcodeDetected;

            var qrcodeVal = "";
            if (e.Results.Length == 0)
            {
                Dispatcher.DispatchAsync(async () =>
                {
                    await DisplayAlert("Error", "No QR code detected.", "OK");
                });

                return;
            }
            else
            {
                qrcodeVal = e.Results[0].Value;
            }

            
            // Send the QR code to the backend for processing

            Dispatcher.DispatchAsync(async () =>
            {
                await VerifyAndProcessTransfer(qrcodeVal);
            });

            Dispatcher.DispatchAsync(async () =>
            {
                DisplayAlert("Info", "Scanning Complete. Press OK to Continue", "OK");
            });

            isProcessingScan = false;
            qrCodeScanner.BarcodesDetected += OnBarcodeDetected;
        }
        catch (Exception ex)
        {
            Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Error", ex.Message, "OK");
            });

        }

    }



    private async Task VerifyAndProcessTransfer(string qrCodeData)
    {
        _currentAccessToken = Preferences.Get("accessToken", string.Empty);
        using (var client = new HttpClient())
        {
            client.SetBearerToken(_currentAccessToken);
            var response = await client.PostAsJsonAsync("https://dev.amnathrig.app/assets/verify-transfer", qrCodeData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var result = JsonSerializer.Deserialize<TransferResponse>(content, options);

                if (result.Message != null)
                {
                    await DisplayAlert("Info", result.Message, "OK");
                }
                else
                {
                    await Navigation.PushAsync(new TransferDetailsPage(result.Asset, result.OwnerUsername));

                    var navigationStack = Navigation.NavigationStack.ToList();
                    if (navigationStack.Count > 1)
                    {
                        Navigation.RemovePage(navigationStack[navigationStack.Count - 2]);
                    }
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to verify transfer.", "OK");
            }
        }
    }

}