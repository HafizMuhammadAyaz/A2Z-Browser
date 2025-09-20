using A2Z_Browser.Services;
using A2Z_Browser.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace A2Z_Browser.Views
{
    public partial class BrowserPage : ContentPage
    {
        private readonly BrowserViewModel _viewModel;
        private IDispatcherTimer _sessionTimer;

        public BrowserPage(BrowserViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Start session timeout (30 minutes)
            _sessionTimer = Dispatcher.CreateTimer();
            _sessionTimer.Interval = TimeSpan.FromMinutes(30);
            _sessionTimer.Tick += OnSessionTimeout;
            _sessionTimer.Start();

            await _viewModel.InitializeBrowser();
            BrowserView.Source = _viewModel.TargetUrl;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_sessionTimer != null)
            {
                _sessionTimer.Stop();
                _sessionTimer.Tick -= OnSessionTimeout;
            }
        }

        private async void OnSessionTimeout(object sender, EventArgs e)
        {
            if (_sessionTimer != null)
            {
                _sessionTimer.Stop();
                _sessionTimer.Tick -= OnSessionTimeout;
            }

            await DisplayAlert("Session Expired", "Your session has timed out. Please login again.", "OK");
            await Shell.Current.GoToAsync("//login");
        }
    }
}