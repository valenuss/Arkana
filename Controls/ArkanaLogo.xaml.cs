namespace Arkana.Controls;

public partial class ArkanaLogo : ContentView
{
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(double), typeof(ArkanaLogo), 32d,
        propertyChanged: (bindable, _, newValue) => ((ArkanaLogo)bindable).LogoLabel.FontSize = (double)newValue);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public ArkanaLogo()
    {
        InitializeComponent();
    }
}