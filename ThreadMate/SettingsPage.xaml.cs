using ThreadMate.Services;

namespace ThreadMate
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            ThemePicker.SelectedIndex = (Application.Current?.UserAppTheme ?? AppTheme.Unspecified) switch
            {
                AppTheme.Light => 1,
                AppTheme.Dark => 2,
                _ => 0
            };

            AppVersionLabel.Text = $"{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";

            // Load banner ad
            AdMobService.LoadBannerAd("SettingsPageBanner", AdContainerGrid);
        }

        private void OnThemeSelectedIndexChanged(object? sender, EventArgs e)
        {
            if (Application.Current is null)
            {
                return;
            }

            Application.Current.UserAppTheme = ThemePicker.SelectedIndex switch
            {
                1 => AppTheme.Light,
                2 => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }

        private async void OnOpenSourceLinkClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { CommandParameter: string url } || string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                await Launcher.Default.OpenAsync(new Uri(url));
            }
            catch
            {
                await DisplayAlert("Unable to open link", "The selected source link could not be opened.", "OK");
            }
        }
    }
}
