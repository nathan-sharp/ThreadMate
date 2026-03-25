using Microsoft.Extensions.DependencyInjection;
#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using WinRT.Interop;
#endif

namespace ThreadMate
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Unspecified;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

#if WINDOWS
            window.Created += (_, _) => ApplyWindowsTitleBarBranding(window);
#endif

            return window;
        }

#if WINDOWS
        private static void ApplyWindowsTitleBarBranding(Window window)
        {
            if (window.Handler?.PlatformView is not MauiWinUIWindow nativeWindow)
            {
                return;
            }

            var hWnd = WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (!AppWindowTitleBar.IsCustomizationSupported())
            {
                return;
            }

            var yellow = Microsoft.UI.ColorHelper.FromArgb(255, 255, 212, 0);
            var yellowHover = Microsoft.UI.ColorHelper.FromArgb(255, 224, 188, 0);
            var yellowPressed = Microsoft.UI.ColorHelper.FromArgb(255, 200, 168, 0);
            var black = Microsoft.UI.Colors.Black;

            var titleBar = appWindow.TitleBar;
            titleBar.BackgroundColor = yellow;
            titleBar.ForegroundColor = black;
            titleBar.InactiveBackgroundColor = yellow;
            titleBar.InactiveForegroundColor = black;

            titleBar.ButtonBackgroundColor = yellow;
            titleBar.ButtonForegroundColor = black;
            titleBar.ButtonHoverBackgroundColor = yellowHover;
            titleBar.ButtonHoverForegroundColor = black;
            titleBar.ButtonPressedBackgroundColor = yellowPressed;
            titleBar.ButtonPressedForegroundColor = black;
            titleBar.ButtonInactiveBackgroundColor = yellow;
            titleBar.ButtonInactiveForegroundColor = black;
        }
#endif
    }
}