using Microsoft.Maui.Controls.Shapes;

namespace Arkana.Controls;

public partial class Avatar : ContentView
{
    public static readonly BindableProperty InitialProperty = BindableProperty.Create(
        nameof(Initial), typeof(string), typeof(Avatar), "?",
        propertyChanged: (bindable, _, newValue) => ((Avatar)bindable).InitialLabel.Text = (string)newValue);

    public static readonly BindableProperty DiameterProperty = BindableProperty.Create(
        nameof(Diameter), typeof(double), typeof(Avatar), 44d,
        propertyChanged: (bindable, _, _) => ((Avatar)bindable).ApplyDiameter());

    public string Initial
    {
        get => (string)GetValue(InitialProperty);
        set => SetValue(InitialProperty, value);
    }

    public double Diameter
    {
        get => (double)GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public Avatar()
    {
        InitializeComponent();
        ApplyDiameter();
    }

    void ApplyDiameter()
    {
        Ring.WidthRequest = Diameter;
        Ring.HeightRequest = Diameter;
        Ring.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(Diameter / 2) };
        Ring.StrokeThickness = Diameter >= 80 ? 3 : 2;
        InitialLabel.FontSize = Diameter * 0.4;
    }
}