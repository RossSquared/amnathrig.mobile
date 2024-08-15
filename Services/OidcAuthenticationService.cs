using IdentityModel.OidcClient;
using System;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;

public class OidcAuthenticationService
{
    private readonly OidcClient _oidcClient;

    public OidcAuthenticationService()
    {
        var options = new OidcClientOptions
        {
            Authority = "https://temp.amnathrig.app",
            ClientId = "maui_client",
            RedirectUri = "mauiapp://callback",
            Scope = "openid profile amnathrigAPI",
            Browser =  new WebViewBrowser()// Use a WebView to handle the login flow

        };

        _oidcClient = new OidcClient(options);
    }

    public async Task<LoginResult> LoginAsync()
    {
        var result = await _oidcClient.LoginAsync(new LoginRequest());

        if (result.IsError)
        {
            // Handle error
            throw new Exception(result.Error);
        }

        // Optionally store tokens in secure storage
        await SecureStorage.SetAsync("access_token", result.AccessToken);
        await SecureStorage.SetAsync("id_token", result.IdentityToken);

        return result;
    }

    public async Task LogoutAsync()
    {
        var idToken = await SecureStorage.GetAsync("id_token");
        var logoutResult = await _oidcClient.LogoutAsync(new LogoutRequest
        {
            IdTokenHint = idToken
        });

        if (logoutResult.IsError)
        {
            // Handle error
            throw new Exception(logoutResult.Error);
        }
    }
}
