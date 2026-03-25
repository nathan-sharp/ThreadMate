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
    }
}
