using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Xamarin.Essentials;
using System.Threading.Tasks;

public class WebViewBrowser : IdentityModel.OidcClient.Browser.IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, System.Threading.CancellationToken cancellationToken = default)
    {
        try
        {
            var authResult = await Xamarin.Essentials.WebAuthenticator.AuthenticateAsync(new Uri(options.StartUrl), new Uri(options.EndUrl));
            return new BrowserResult
            {
                Response = authResult?.Properties["code"],
                ResultType = BrowserResultType.Success
            };
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel
            };
        }
        catch (Exception ex)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = ex.Message
            };
        }
    }
}
