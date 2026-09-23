namespace Orkest.Stoelenplan.Shared.Models;

public enum InstrumentGroep
{
    Strijkers,
    Houtblazers,
    Saxofoons,
    Koper,
    Slagwerk,
    Ritmesectie,
    Overig,
}

/// <summary>
/// Een instrument waar stoelen en musici bij horen. De standaardinstrumenten staan in
/// <see cref="StandaardInstrumenten"/>; gebruikers kunnen er eigen instrumenten bij maken.
/// </summary>
public sealed class Instrument
{
    /// <summary>Vaste sleutel waarmee stoelen en musici naar het instrument verwijzen.</summary>
    public string Id { get; set; } = "";
    public string Naam { get; set; } = "";

    /// <summary>Korte aanduiding die op een stoel in het plan past, bv. "Kl" of "A.sax".</summary>
    public string Afkorting { get; set; } = "";
    public InstrumentGroep Groep { get; set; } = InstrumentGroep.Overig;

    /// <summary>Achtergrondkleur van de stoel als hex-code; licht genoeg voor zwarte tekst.</summary>
    public string Kleur { get; set; } = "#DDDDDD";

    public override string ToString() => Naam;
}
