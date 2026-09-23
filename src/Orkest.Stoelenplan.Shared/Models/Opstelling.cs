namespace Orkest.Stoelenplan.Shared.Models;

/// <summary>
/// Een complete stoelenplan-opstelling: welke musici er meespelen en waar de stoelen staan.
/// </summary>
public sealed class Opstelling
{
    public const double PodiumBreedte = 1200;
    public const double PodiumHoogte = 700;

    /// <summary>Plek van de dirigent: midden onderaan het podium, het publiek zit "onder" het plan.</summary>
    public const double DirigentX = PodiumBreedte / 2;
    public const double DirigentY = PodiumHoogte - 60;

    public string Naam { get; set; } = "";
    public List<Musicus> Musici { get; set; } = [];
    public List<Stoel> Stoelen { get; set; } = [];

    /// <summary>
    /// Eigen (niet-standaard) instrumenten die in deze opstelling gebruikt worden. Zo blijft
    /// een opstelling bruikbaar op een andere computer die die instrumenten nog niet kent.
    /// </summary>
    public List<Instrument> EigenInstrumenten { get; set; } = [];
}
