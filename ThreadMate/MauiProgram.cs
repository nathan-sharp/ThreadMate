using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using ThreadMate.Services;
#if ANDROID
using Android.Graphics;
using Android.Views;
using Android.Widget;
#endif

namespace ThreadMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SpaceMono-Regular.ttf", "SpaceMonoRegular");
                    fonts.AddFont("SpaceMono-Bold.ttf", "SpaceMonoBold");
                })
                // Register AdMob Service
                .Services.AddSingleton<AdMobService>();

#if ANDROID
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddAndroid(android =>
                {
                    android.OnPostCreate((activity, _) =>
                    {
                        ApplySpaceMonoToAndroidViewTree(activity.Window?.DecorView, activity.Assets);
                    });

                    android.OnResume(activity =>
                    {
                        activity.Window?.DecorView?.Post(() =>
                        {
                            ApplySpaceMonoToAndroidViewTree(activity.Window?.DecorView, activity.Assets);
                        });
                    });
                });
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

#if ANDROID
        private static void ApplySpaceMonoToAndroidViewTree(Android.Views.View? root, Android.Content.Res.AssetManager assets)
        {
            if (root is null)
            {
                return;
            }

            var typeface = Typeface.CreateFromAsset(assets, "SpaceMono-Regular.ttf");
            ApplyTypefaceRecursively(root, typeface);
        }

        private static void ApplyTypefaceRecursively(Android.Views.View view, Typeface typeface)
        {
            if (view is TextView textView)
            {
                textView.Typeface = typeface;
            }

            if (view is not ViewGroup group)
            {
                return;
            }

            for (var i = 0; i < group.ChildCount; i++)
            {
                var child = group.GetChildAt(i);
                if (child is not null)
                {
                    ApplyTypefaceRecursively(child, typeface);
                }
            }
        }
#endif
    }
}
