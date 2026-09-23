namespace Orkest.Stoelenplan.Shared.Models;

/// <summary>
/// Instrumentgroep waar een stoel (en een musicus) bij hoort. Wordt als tekst
/// opgeslagen in JSON, dus de namen hier niet zomaar hernoemen.
/// </summary>
public enum Sectie
{
    Viool1,
    Viool2,
    Altviool,
    Cello,
    Contrabas,
    Fluit,
    Hobo,
    Klarinet,
    Fagot,
    Hoorn,
    Trompet,
    Trombone,
    Tuba,
    Slagwerk,
    Harp,
    Piano,
}

public static class SectieExtensions
{
    public static string Weergavenaam(this Sectie sectie) => sectie switch
    {
        Sectie.Viool1 => "1e viool",
        Sectie.Viool2 => "2e viool",
        _ => sectie.ToString(),
    };

    /// <summary>Korte aanduiding die op een stoel in het plan past.</summary>
    public static string Afkorting(this Sectie sectie) => sectie switch
    {
        Sectie.Viool1 => "Vl 1",
        Sectie.Viool2 => "Vl 2",
        Sectie.Altviool => "Alt",
        Sectie.Cello => "Vc",
        Sectie.Contrabas => "Cb",
        Sectie.Fluit => "Fl",
        Sectie.Hobo => "Hb",
        Sectie.Klarinet => "Kl",
        Sectie.Fagot => "Fg",
        Sectie.Hoorn => "Hn",
        Sectie.Trompet => "Tp",
        Sectie.Trombone => "Tb",
        Sectie.Tuba => "Tu",
        Sectie.Slagwerk => "Slw",
        Sectie.Harp => "Hp",
        Sectie.Piano => "Pno",
        _ => sectie.ToString(),
    };
}
