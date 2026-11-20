namespace Arkana.Helpers;

public static class DateTimeExtensions
{
    // "ahora", "hace 5 min", "hace 2 h", "hace 3 d"
    public static string ToTimeAgo(this DateTime date)
    {
        var elapsed = DateTime.Now - date;

        return elapsed switch
        {
            { TotalMinutes: < 1 } => "ahora",
            { TotalHours: < 1 } => $"hace {(int)elapsed.TotalMinutes} min",
            { TotalDays: < 1 } => $"hace {(int)elapsed.TotalHours} h",
            _ => $"hace {(int)elapsed.TotalDays} d"
        };
    }
}