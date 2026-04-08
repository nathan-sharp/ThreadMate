using System.Diagnostics;
using ThreadMate.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace ThreadMate.Services
{
    /// <summary>
    /// Service for managing Google AdMob banner ads across the application.
    /// Uses Google's test ad unit IDs for development and testing.
    /// </summary>
    public class AdMobService
    {
        private static readonly Dictionary<string, View?> _adContainers = new();

        // Google AdMob Test Ad Unit IDs (safe for development)
        // These IDs are provided by Google for testing purposes
        private const string AndroidBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        private const string iOSBannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";
        private const string WindowsBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";

        /// <summary>
        /// Get the appropriate banner ad unit ID based on platform
        /// </summary>
        private static string GetBannerAdUnitId()
        {
#if __ANDROID__
            return AndroidBannerAdUnitId;
#elif __IOS__
            return iOSBannerAdUnitId;
#elif WINDOWS
            return WindowsBannerAdUnitId;
#else
            return AndroidBannerAdUnitId;
#endif
        }

        /// <summary>
        /// Load and display a banner ad in the specified container.
        /// Call this from your page's OnAppearing or constructor.
        /// </summary>
        /// <param name="containerName">Unique identifier for the ad container (e.g., "MainPageBanner")</param>
        /// <param name="container">The View container where the ad will be displayed (usually a Grid or StackLayout)</param>
        public static void LoadBannerAd(string containerName, View container)
        {
            try
            {
                // Store the container for later reference
                _adContainers[containerName] = container;

                // Create the ad content (real ad or placeholder)
                CreateBannerContent(container);

                Debug.WriteLine($"[AdMob] Banner initialized for container: {containerName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdMob] Error loading banner ad: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the banner content - either real AdMob ad (Android) or placeholder (iOS, Windows).
        /// </summary>
        private static void CreateBannerContent(View container)
        {
            if (container is Grid grid)
            {
                grid.Children.Clear();
#if ANDROID
                var wrapper = new Grid();
                wrapper.Children.Add(CreateBannerPlaceholder());
                wrapper.Children.Add(new AdMobBannerView
                {
                    AdUnitId = GetBannerAdUnitId(),
                    HeightRequest = 50,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center
                });
                grid.Children.Add(wrapper);
#else
                grid.Children.Add(CreateBannerPlaceholder());
#endif
                return;
            }

            if (container is VerticalStackLayout vsl)
            {
                vsl.Children.Clear();
#if ANDROID
                var wrapper = new Grid();
                wrapper.Children.Add(CreateBannerPlaceholder());
                wrapper.Children.Add(new AdMobBannerView
                {
                    AdUnitId = GetBannerAdUnitId(),
                    HeightRequest = 50,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center
                });
                vsl.Children.Add(wrapper);
#else
                vsl.Children.Add(CreateBannerPlaceholder());
#endif
            }
        }

        /// <summary>
        /// Creates a visual placeholder for the banner ad.
        /// In production, replace with actual Google Mobile Ads SDK implementation.
        /// </summary>
        private static View CreateBannerPlaceholder()
        {
            var bannerContainer = new Border
            {
                Stroke = Colors.Gray,
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                StrokeThickness = 1,
                Padding = 8,
                Margin = 0,
                HeightRequest = 50
            };

            var bannerLabel = new Label
            {
                Text = "📢 Advertisement",
                FontSize = 12,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Italic
            };

            bannerContainer.Content = bannerLabel;
            return bannerContainer;
        }

        /// <summary>
        /// Remove a banner ad from display
        /// </summary>
        public static void RemoveBannerAd(string containerName)
        {
            if (_adContainers.TryGetValue(containerName, out var container))
            {
                if (container is Grid grid)
                {
                    grid.Children.Clear();
                }
                else if (container is VerticalStackLayout vsl)
                {
                    vsl.Children.Clear();
                }

                _adContainers.Remove(containerName);
                Debug.WriteLine($"[AdMob] Banner ad removed from container: {containerName}");
            }
        }

        /// <summary>
        /// Clear all ad containers
        /// </summary>
        public static void ClearAllAds()
        {
            foreach (var containerName in _adContainers.Keys.ToList())
            {
                RemoveBannerAd(containerName);
            }
        }

        /// <summary>
        /// Get banner ad unit ID for the current platform
        /// (Use this when integrating with the actual Google Mobile Ads SDK)
        /// </summary>
        public static string GetCurrentPlatformBannerAdUnitId() => GetBannerAdUnitId();
    }
}
