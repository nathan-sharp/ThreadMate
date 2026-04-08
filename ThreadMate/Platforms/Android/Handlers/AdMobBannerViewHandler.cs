using Android.Gms.Ads;
using Android.Views;
using Microsoft.Maui.Handlers;
using ThreadMate.Controls;

namespace ThreadMate.Platforms.Android.Handlers;

public class AdMobBannerViewHandler : ViewHandler<AdMobBannerView, AdView>
{
    public static readonly PropertyMapper<AdMobBannerView, AdMobBannerViewHandler> Mapper =
        new(ViewMapper)
        {
            [nameof(AdMobBannerView.AdUnitId)] = MapAdUnitId
        };

    public AdMobBannerViewHandler() : base(Mapper)
    {
    }

    protected override AdView CreatePlatformView()
    {
        MobileAds.Initialize(Context);

        var adView = new AdView(Context)
        {
            AdSize = AdSize.Banner,
            Visibility = ViewStates.Gone
        };

        adView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

        return adView;
    }

    public static void MapAdUnitId(AdMobBannerViewHandler handler, AdMobBannerView view)
    {
        if (handler.PlatformView is null || string.IsNullOrWhiteSpace(view.AdUnitId))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(handler.PlatformView.AdUnitId))
        {
            handler.PlatformView.AdUnitId = view.AdUnitId;
        }

        handler.PlatformView.AdListener = new BannerAdListener(handler.PlatformView);
        handler.PlatformView.LoadAd(new AdRequest.Builder().Build());
    }

    protected override void DisconnectHandler(AdView platformView)
    {
        platformView.Destroy();
        base.DisconnectHandler(platformView);
    }

    private sealed class BannerAdListener(AdView adView) : AdListener
    {
        public override void OnAdLoaded()
        {
            adView.Visibility = ViewStates.Visible;
            base.OnAdLoaded();
        }

        public override void OnAdFailedToLoad(LoadAdError? error)
        {
            adView.Visibility = ViewStates.Gone;
            base.OnAdFailedToLoad(error);
        }
    }
}
