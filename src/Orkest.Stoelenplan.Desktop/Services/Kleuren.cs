using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Orkest.Stoelenplan.Desktop.Services;

public static class Kleuren
{
    private static readonly IBrush Reserve = new ImmutableSolidColorBrush(Color.Parse("#DDDDDD"));
    private static readonly Dictionary<string, IBrush> Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Keuzekleuren voor eigen instrumenten: licht genoeg om zwarte tekst op te lezen en
    /// goed te onderscheiden van elkaar.
    /// </summary>
    public static IReadOnlyList<string> Palet { get; } =
    [
        "#F4A6A6", "#F7C3A1", "#FFE08A", "#D9EBA0", "#A8D8B0", "#9FD3C7",
        "#A9CCE8", "#B7C4F2", "#D2B8EE", "#F2C6DE", "#E6C8B0", "#C9CED6",
    ];

    /// <summary>Brush voor een hex-kleur uit een instrument; onbruikbare waarden worden grijs.</summary>
    public static IBrush Brush(string hex)
    {
        if (Cache.TryGetValue(hex, out var brush))
        {
            return brush;
        }

        brush = Color.TryParse(hex, out var kleur) ? new ImmutableSolidColorBrush(kleur) : Reserve;
        Cache[hex] = brush;
        return brush;
    }
}
