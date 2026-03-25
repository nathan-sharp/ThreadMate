namespace ThreadMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            MainTabBar.CurrentItem = ThreadInfoTab;
        }
    }
}
