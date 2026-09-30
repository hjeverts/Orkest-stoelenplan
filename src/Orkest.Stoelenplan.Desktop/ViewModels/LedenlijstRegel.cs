using Orkest.Stoelenplan.Shared.Models;

namespace Orkest.Stoelenplan.Desktop.ViewModels;

/// <summary>Eén regel in de ledenlijst: een musicus met zijn instrument en stem (partij).</summary>
public sealed record LedenlijstRegel(string Naam, Instrument Instrument, string Stem);
