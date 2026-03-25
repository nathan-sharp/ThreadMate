namespace ThreadMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            MainTabBar.CurrentItem = ThreadInfoTab;
        }

        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            await GoToAsync(nameof(SettingsPage));
        }
    }
}
