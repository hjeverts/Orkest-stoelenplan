using System.Collections.ObjectModel;
using Orkest.Stoelenplan.Shared.Models;

namespace Orkest.Stoelenplan.Desktop.Services;

/// <summary>
/// Alle instrumenten die in de app te kiezen zijn: de standaardinstrumenten plus de eigen
/// instrumenten, gesorteerd op groep. Keuzelijsten binden direct aan <see cref="Alle"/>.
/// </summary>
public sealed class InstrumentenCatalogus
{
    // Instrumenten die in een opstelling voorkomen maar nergens bekend zijn; per id één
    // vast object, zodat stoelen en musici met hetzelfde onbekende instrument bij elkaar horen.
    private readonly Dictionary<string, Instrument> _onbekend = [];

    public ObservableCollection<Instrument> Alle { get; } = new(StandaardInstrumenten.Alle);

    public IReadOnlyList<Instrument> Eigen => Alle.Where(IsEigen).ToList();

    public static bool IsEigen(Instrument instrument) => !StandaardInstrumenten.IsStandaard(instrument.Id);

    public Instrument Zoek(string id)
    {
        if (Alle.FirstOrDefault(i => i.Id == id) is { } instrument)
        {
            return instrument;
        }

        if (!_onbekend.TryGetValue(id, out var onbekend))
        {
            onbekend = new Instrument { Id = id, Naam = $"Onbekend ({id})", Afkorting = "?" };
            _onbekend[id] = onbekend;
        }
        return onbekend;
    }

    public bool Bestaat(string id) => Alle.Any(i => i.Id == id);

    /// <summary>Plek in de catalogus, om musici op instrumentvolgorde te kunnen sorteren.</summary>
    public int Volgorde(Instrument instrument)
    {
        var index = Alle.IndexOf(instrument);
        return index < 0 ? int.MaxValue : index;
    }

    /// <summary>Voegt een eigen instrument toe achteraan zijn groep.</summary>
    public void VoegToe(Instrument instrument)
    {
        if (Bestaat(instrument.Id))
        {
            return;
        }

        var index = Alle.Count;
        for (var i = Alle.Count - 1; i >= 0; i--)
        {
            if (Alle[i].Groep <= instrument.Groep)
            {
                index = i + 1;
                break;
            }
            index = i;
        }
        Alle.Insert(index, instrument);
    }

    public void Verwijder(Instrument instrument)
    {
        if (IsEigen(instrument))
        {
            Alle.Remove(instrument);
        }
    }
}
