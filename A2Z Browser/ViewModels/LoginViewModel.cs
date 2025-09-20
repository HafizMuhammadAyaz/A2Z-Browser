using A2Z_Browser.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace A2Z_Browser.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                HasError = false;

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter both username and password";
                    HasError = true;
                    return;
                }

                var success = await _authService.AuthenticateUser(Username, Password);

                if (success)
                {
                    // Update last login time
                    await _authService.UpdateUserLastLogin(Username);

                    // Navigate to browser page
                    await Shell.Current.GoToAsync("//browser");
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}