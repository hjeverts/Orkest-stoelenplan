using Avalonia.Media;
using Avalonia.Media.Immutable;
using Crescendo.Stoelenplan.Shared.Models;

namespace Crescendo.Stoelenplan.Desktop.Services;

/// <summary>
/// Kleur per sectie: strijkers in warme tinten, houtblazers groen, koper geel/oranje,
/// slagwerk en overig grijs/paars. Licht genoeg om zwarte tekst op te lezen.
/// </summary>
public static class SectieKleuren
{
    private static readonly Dictionary<Sectie, IBrush> Kleuren = new()
    {
        [Sectie.Viool1] = Brush("#F4A6A6"),
        [Sectie.Viool2] = Brush("#F7C3A1"),
        [Sectie.Altviool] = Brush("#F3D9A4"),
        [Sectie.Cello] = Brush("#D9B8E8"),
        [Sectie.Contrabas] = Brush("#C3B1E1"),
        [Sectie.Fluit] = Brush("#BFE3C0"),
        [Sectie.Hobo] = Brush("#A8D8B0"),
        [Sectie.Klarinet] = Brush("#9FD3C7"),
        [Sectie.Fagot] = Brush("#B5D99C"),
        [Sectie.Hoorn] = Brush("#FFE08A"),
        [Sectie.Trompet] = Brush("#FFD06B"),
        [Sectie.Trombone] = Brush("#FFC078"),
        [Sectie.Tuba] = Brush("#F5B971"),
        [Sectie.Slagwerk] = Brush("#C9CED6"),
        [Sectie.Harp] = Brush("#A9CCE8"),
        [Sectie.Piano] = Brush("#B8C4E0"),
    };

    public static IBrush Voor(Sectie sectie) => Kleuren[sectie];

    private static IBrush Brush(string hex) => new ImmutableSolidColorBrush(Color.Parse(hex));
}
