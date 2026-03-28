using System.Diagnostics;

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
        private const string AndroidBannerTestId = "ca-app-pub-8060016937826958/6363913816";
        private const string iOSBannerTestId = "ca-app-pub-3940256099942544/2934735716";
        private const string WindowsBannerTestId = "ca-app-pub-3940256099942544/6300978111";

        /// <summary>
        /// Get the appropriate banner ad unit ID based on platform
        /// </summary>
        private static string GetBannerAdUnitId()
        {
#if __ANDROID__
            return AndroidBannerTestId;
#elif __IOS__
            return iOSBannerTestId;
#elif WINDOWS
            return WindowsBannerTestId;
#else
            return AndroidBannerTestId;
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

                // Create a placeholder banner that simulates an ad
                // In production, this would use the actual Google Mobile Ads SDK
                CreateBannerPlaceholder(containerName, container);

                Debug.WriteLine($"[AdMob] Banner ad loaded for container: {containerName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdMob] Error loading banner ad: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a visual placeholder for the banner ad.
        /// In production, replace with actual Google Mobile Ads SDK implementation.
        /// </summary>
        private static void CreateBannerPlaceholder(string containerName, View container)
        {
            // Clear existing content
            if (container is Grid grid)
            {
                grid.Children.Clear();
            }
            else if (container is VerticalStackLayout vsl)
            {
                vsl.Children.Clear();
            }

            // Create a banner-sized placeholder
            var bannerContainer = new Frame
            {
                BorderColor = Colors.Gray,
                BackgroundColor = Colors.White,
                CornerRadius = 4,
                HasShadow = false,
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

            // Add to the container
            if (container is Grid grid2)
            {
                grid2.Add(bannerContainer);
            }
            else if (container is VerticalStackLayout vsl2)
            {
                vsl2.Add(bannerContainer);
            }
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
