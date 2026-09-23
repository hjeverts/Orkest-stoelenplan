namespace Orkest.Stoelenplan.Shared.Models;

/// <summary>
/// Eén stoel op het podium. <see cref="X"/> en <see cref="Y"/> zijn het middelpunt van
/// de stoel in podiumcoördinaten (zie <see cref="Opstelling.PodiumBreedte"/>/<see cref="Opstelling.PodiumHoogte"/>).
/// </summary>
public sealed class Stoel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Sectie Sectie { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    /// <summary>De musicus die op deze stoel zit, of <c>null</c> als de stoel nog leeg is.</summary>
    public Guid? MusicusId { get; set; }
}
