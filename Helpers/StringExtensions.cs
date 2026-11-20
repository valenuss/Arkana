namespace Arkana.Helpers;

public static class StringExtensions
{
    // "luna" → "L", o "?" si no hay texto
    public static string ToInitial(this string? text) =>
        string.IsNullOrWhiteSpace(text) ? "?" : text.Trim()[..1].ToUpperInvariant();
}