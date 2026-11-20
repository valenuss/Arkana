namespace Arkana.Controls;

public partial class FilterChip : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(FilterChip), string.Empty);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(string), typeof(FilterChip), string.Empty,
        propertyChanged: OnSelectionChanged);

    public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create(
        nameof(SelectedValue), typeof(string), typeof(FilterChip), string.Empty,
        BindingMode.TwoWay, propertyChanged: OnSelectionChanged);

    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(FilterChip), false);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string SelectedValue
    {
        get => (string)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        private set => SetValue(IsActiveProperty, value);
    }

    public FilterChip()
    {
        InitializeComponent();
    }

    static void OnSelectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var chip = (FilterChip)bindable;
        chip.IsActive = chip.Value == chip.SelectedValue;
    }

    void Chip_Tapped(object? sender, TappedEventArgs e) => SelectedValue = Value;
}