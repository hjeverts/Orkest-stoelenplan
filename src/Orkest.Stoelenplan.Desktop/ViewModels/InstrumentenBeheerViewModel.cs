using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Orkest.Stoelenplan.Desktop.Services;
using Orkest.Stoelenplan.Shared.Models;
using Orkest.Stoelenplan.Shared.Opslag;

namespace Orkest.Stoelenplan.Desktop.ViewModels;

public sealed record KleurOptie(string Hex)
{
    public IBrush Brush => Kleuren.Brush(Hex);
}

/// <summary>Eén regel in de instrumentenlijst van het beheerscherm.</summary>
public partial class InstrumentRegel(Instrument instrument, Func<InstrumentRegel, Task> verwijder) : ObservableObject
{
    public Instrument Instrument => instrument;
    public string Naam => instrument.Naam;
    public string Afkorting => instrument.Afkorting;
    public string Groep => instrument.Groep.ToString();
    public IBrush Kleur => Kleuren.Brush(instrument.Kleur);
    public bool IsEigen => InstrumentenCatalogus.IsEigen(instrument);
    public string Soort => IsEigen ? "eigen" : "standaard";

    [RelayCommand(CanExecute = nameof(IsEigen))]
    private Task VerwijderenAsync() => verwijder(this);
}

/// <summary>
/// Scherm om eigen instrumenten toe te voegen of te verwijderen. Wijzigingen worden meteen
/// in de catalogus doorgevoerd (dus ook in de keuzelijsten van het hoofdscherm) en opgeslagen.
/// </summary>
public partial class InstrumentenBeheerViewModel : ObservableObject
{
    private readonly InstrumentenCatalogus _catalogus;
    private readonly IOpstellingOpslag _opslag;
    private readonly Func<Instrument, bool> _isInGebruik;

    public InstrumentenBeheerViewModel(InstrumentenCatalogus catalogus, IOpstellingOpslag opslag, Func<Instrument, bool> isInGebruik)
    {
        _catalogus = catalogus;
        _opslag = opslag;
        _isInGebruik = isInGebruik;
        _kleur = KleurOpties[0];
        VerversRegels();
    }

    public ObservableCollection<InstrumentRegel> Regels { get; } = [];
    public IReadOnlyList<InstrumentGroep> Groepen { get; } = Enum.GetValues<InstrumentGroep>();
    public IReadOnlyList<KleurOptie> KleurOpties { get; } = Kleuren.Palet.Select(h => new KleurOptie(h)).ToList();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToevoegenCommand))]
    private string _naam = "";

    [ObservableProperty]
    private string _afkorting = "";

    [ObservableProperty]
    private InstrumentGroep _groep = InstrumentGroep.Overig;

    [ObservableProperty]
    private KleurOptie _kleur;

    [ObservableProperty]
    private string _statusMessage = "Standaardinstrumenten kunnen niet verwijderd worden; eigen instrumenten wel.";

    private bool KanToevoegen() => !string.IsNullOrWhiteSpace(Naam);

    [RelayCommand(CanExecute = nameof(KanToevoegen))]
    private async Task ToevoegenAsync()
    {
        var naam = Naam.Trim();
        if (_catalogus.Alle.Any(i => string.Equals(i.Naam, naam, StringComparison.CurrentCultureIgnoreCase)))
        {
            StatusMessage = $"Er bestaat al een instrument \"{naam}\".";
            return;
        }

        var afkorting = Afkorting.Trim();
        var instrument = new Instrument
        {
            Id = "eigen-" + Guid.NewGuid().ToString("N")[..12],
            Naam = naam,
            Afkorting = afkorting.Length > 0 ? afkorting : naam[..Math.Min(4, naam.Length)],
            Groep = Groep,
            Kleur = Kleur.Hex,
        };

        _catalogus.VoegToe(instrument);
        VerversRegels();
        Naam = "";
        Afkorting = "";
        Kleur = KleurOpties[(KleurOpties.ToList().IndexOf(Kleur) + 1) % KleurOpties.Count];

        StatusMessage = await BewaarAsync()
            ? $"\"{instrument.Naam}\" toegevoegd."
            : StatusMessage;
    }

    private async Task VerwijderAsync(InstrumentRegel regel)
    {
        if (_isInGebruik(regel.Instrument))
        {
            StatusMessage = $"\"{regel.Naam}\" wordt nog gebruikt door een stoel of musicus in deze opstelling.";
            return;
        }

        _catalogus.Verwijder(regel.Instrument);
        VerversRegels();
        StatusMessage = await BewaarAsync()
            ? $"\"{regel.Naam}\" verwijderd."
            : StatusMessage;
    }

    private async Task<bool> BewaarAsync()
    {
        try
        {
            await _opslag.OpslaanEigenInstrumentenAsync(_catalogus.Eigen);
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Opslaan van de instrumenten mislukt: {ex.Message}";
            return false;
        }
    }

    private void VerversRegels()
    {
        Regels.Clear();
        foreach (var instrument in _catalogus.Alle)
        {
            Regels.Add(new InstrumentRegel(instrument, VerwijderAsync));
        }
    }
}
