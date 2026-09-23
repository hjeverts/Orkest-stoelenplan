using Crescendo.Stoelenplan.Shared.Models;

namespace Crescendo.Stoelenplan.Desktop.ViewModels;

/// <summary>Sectie zoals hij in een keuzelijst verschijnt ("1e viool" in plaats van "Viool1").</summary>
public sealed record SectieOptie(Sectie Sectie)
{
    public static IReadOnlyList<SectieOptie> Alle { get; } =
        Enum.GetValues<Sectie>().Select(s => new SectieOptie(s)).ToList();

    public override string ToString() => Sectie.Weergavenaam();
}
