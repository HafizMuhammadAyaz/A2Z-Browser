using A2Z_Browser.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace A2Z_Browser.ViewModels
{
    public partial class BrowserViewModel : ObservableObject
    {
        private readonly ProxyService _proxyService;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string targetUrl;

        public BrowserViewModel(ProxyService proxyService)
        {
            _proxyService = proxyService;
            TargetUrl = "https://highway.com/highway-for-carriers";
        }

        public async Task InitializeBrowser()
        {
            IsLoading = true;

            try
            {
                var proxyConfig = await _proxyService.GetProxyConfiguration();

                if (proxyConfig == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No proxy configuration found", "OK");
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                var isValid = await _proxyService.VerifyProxy(proxyConfig);

                // Update proxy status in database
                await _proxyService.UpdateProxyStatus(proxyConfig, isValid);

                if (!isValid)
                {
                    await Shell.Current.DisplayAlert("Proxy Error",
                        "The configured proxy is not working. Please contact administrator.", "OK");
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                // If we had a way to set the proxy at the WebView level, we would do it here
                // Currently, MAUI WebView doesn't support custom proxies directly
                // This would require a custom implementation or platform-specific code
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    $"Failed to initialize browser: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("//login");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}