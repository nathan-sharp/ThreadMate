namespace ThreadMate.Controls;

public class AdMobBannerView : View
{
    public static readonly BindableProperty AdUnitIdProperty =
        BindableProperty.Create(nameof(AdUnitId), typeof(string), typeof(AdMobBannerView), string.Empty);

    public string AdUnitId
    {
        get => (string)GetValue(AdUnitIdProperty);
        set => SetValue(AdUnitIdProperty, value);
    }
}
