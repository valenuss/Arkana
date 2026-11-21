using Microsoft.Maui.Controls.Shapes;

namespace Arkana.Controls;

/// <summary>Círculo con la inicial del usuario; el color del borde depende de la letra.</summary>
public partial class Avatar : ContentView
{
    static readonly Color[] Palette =
    {
        Color.FromArgb("#FF1F3D"),
        Color.FromArgb("#FF7A1F"),
        Color.FromArgb("#FFC21F"),
        Color.FromArgb("#2ECC71"),
        Color.FromArgb("#1FB6FF"),
        Color.FromArgb("#7A5CFF"),
        Color.FromArgb("#E91E8C"),
        Color.FromArgb("#1FE0C0")
    };

    public static readonly BindableProperty InitialProperty = BindableProperty.Create(
        nameof(Initial), typeof(string), typeof(Avatar), "?",
        propertyChanged: (bindable, _, newValue) => ((Avatar)bindable).ApplyInitial((string?)newValue));

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

    // pone la letra y el color de su borde
    void ApplyInitial(string? initial)
    {
        InitialLabel.Text = initial;
        Ring.Stroke = new SolidColorBrush(ColorFor(initial));
    }

    static Color ColorFor(string? initial)
    {
        if (string.IsNullOrEmpty(initial))
        {
            return Palette[0];
        }

        return Palette[char.ToUpperInvariant(initial[0]) % Palette.Length];
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