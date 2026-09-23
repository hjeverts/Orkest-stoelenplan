using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Orkest.Stoelenplan.Desktop.Services;
using Orkest.Stoelenplan.Shared.Models;
using Orkest.Stoelenplan.Shared.Opslag;

namespace Orkest.Stoelenplan.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private static readonly StringComparer NaamComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("nl-NL"), ignoreCase: true);

    private readonly IOpstellingOpslag _opslag;

    public MainWindowViewModel(IOpstellingOpslag opslag)
    {
        _opslag = opslag;
        _nieuweMusicusInstrument = Catalogus.Alle[0];
        _nieuweStoelInstrument = Catalogus.Alle[0];
        ZetStoelen(StandaardOpstellingen.Maak(GekozenOrkestType));
    }

    public InstrumentenCatalogus Catalogus { get; } = new();
    public ObservableCollection<Instrument> Instrumenten => Catalogus.Alle;
    public ObservableCollection<StoelViewModel> Stoelen { get; } = [];
    public ObservableCollection<MusicusViewModel> Musici { get; } = [];
    public ObservableCollection<string> OpgeslagenOpstellingen { get; } = [];
    public IReadOnlyList<OrkestType> OrkestTypes { get; } = Enum.GetValues<OrkestType>();

    [ObservableProperty]
    private OrkestType _gekozenOrkestType = OrkestType.Symfonieorkest;

    [ObservableProperty]
    private string _opstellingNaam = "Nieuwe opstelling";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenenCommand))]
    private string? _geselecteerdeOpstelling;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MusicusToevoegenCommand))]
    private string _nieuweMusicusNaam = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MusicusToevoegenCommand))]
    private Instrument? _nieuweMusicusInstrument;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StoelToevoegenCommand))]
    private Instrument? _nieuweStoelInstrument;

    [ObservableProperty]
    private string _nieuweStoelPartij = "";

    [ObservableProperty]
    private string _statusMessage = "Voeg musici toe en sleep ze naar een stoel. Stoelen zelf kun je ook verslepen.";

    [ObservableProperty]
    private string _plaatsingSamenvatting = "";

    public async Task InitialiseerAsync()
    {
        try
        {
            foreach (var instrument in await _opslag.EigenInstrumentenAsync())
            {
                Catalogus.VoegToe(instrument);
            }
            await VerversOpgeslagenAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kon opgeslagen gegevens niet ophalen: {ex.Message}";
        }
        WerkPlaatsingBij();
    }

    public InstrumentenBeheerViewModel MaakInstrumentenBeheer()
        => new(Catalogus, _opslag, IsInGebruik);

    private bool IsInGebruik(Instrument instrument)
        => Stoelen.Any(s => s.Instrument == instrument) || Musici.Any(m => m.Instrument == instrument);

    /// <summary>
    /// Zet een musicus op een stoel. Zat de musicus al ergens, dan komt die stoel vrij;
    /// zat er al iemand op de doelstoel, dan wordt die weer "niet geplaatst".
    /// </summary>
    public void Plaats(MusicusViewModel musicus, StoelViewModel stoel)
    {
        foreach (var andere in Stoelen.Where(s => s.Musicus == musicus && s != stoel))
        {
            andere.Musicus = null;
        }
        stoel.Musicus = musicus;

        if (musicus.Instrument != stoel.Instrument)
        {
            StatusMessage = $"Let op: {musicus.Naam} ({musicus.InstrumentNaam}) zit nu op een stoel voor {stoel.Instrument.Naam}.";
        }
    }

    public MusicusViewModel? ZoekMusicus(Guid id) => Musici.FirstOrDefault(m => m.Id == id);

    private bool KanMusicusToevoegen() => !string.IsNullOrWhiteSpace(NieuweMusicusNaam) && NieuweMusicusInstrument is not null;

    [RelayCommand(CanExecute = nameof(KanMusicusToevoegen))]
    private void MusicusToevoegen()
    {
        VoegMusicusToe(new Musicus { Naam = NieuweMusicusNaam.Trim(), InstrumentId = NieuweMusicusInstrument!.Id });
        NieuweMusicusNaam = "";
        WerkPlaatsingBij();
    }

    private bool KanStoelToevoegen() => NieuweStoelInstrument is not null;

    [RelayCommand(CanExecute = nameof(KanStoelToevoegen))]
    private void StoelToevoegen()
    {
        // Nieuwe stoelen komen linksboven op het podium; schuif ze een beetje op zodat
        // meerdere nieuwe stoelen niet precies op elkaar liggen.
        var verschuiving = Stoelen.Count % 8 * 12;
        VoegStoelToe(new Stoel
        {
            InstrumentId = NieuweStoelInstrument!.Id,
            Partij = NieuweStoelPartij.Trim(),
            X = 60 + verschuiving,
            Y = 60 + verschuiving,
        });
        WerkPlaatsingBij();
    }

    [RelayCommand]
    private void Standaardopstelling()
    {
        ZetStoelen(StandaardOpstellingen.Maak(GekozenOrkestType));
        StatusMessage = $"Standaardopstelling voor een {GekozenOrkestType.ToString().ToLowerInvariant()} neergezet; alle stoelen zijn weer leeg.";
    }

    /// <summary>
    /// Zet iedereen die nog niet zit op een lege stoel van het eigen instrument, in de
    /// volgorde van de musicilijst: eerst de hoogste partij (Solo, 1, Rep, 2, …) en binnen
    /// een partij van voor (dichtbij de dirigent) naar achter.
    /// </summary>
    [RelayCommand]
    private void AutomatischIndelen()
    {
        var vrijeStoelen = Stoelen
            .Where(s => s.Musicus is null)
            .OrderBy(s => PartijRang(s.Partij))
            .ThenBy(s => Math.Pow(s.X - Opstelling.DirigentX, 2) + Math.Pow(s.Y - Opstelling.DirigentY, 2))
            .ToList();

        var geenPlek = new List<string>();
        foreach (var musicus in Musici.Where(m => !m.IsGeplaatst).ToList())
        {
            var stoel = vrijeStoelen.FirstOrDefault(s => s.Instrument == musicus.Instrument);
            if (stoel is null)
            {
                geenPlek.Add(musicus.Naam);
                continue;
            }
            stoel.Musicus = musicus;
            vrijeStoelen.Remove(stoel);
        }

        StatusMessage = geenPlek.Count == 0
            ? "Iedereen is ingedeeld."
            : $"Geen vrije stoel voor het eigen instrument van: {string.Join(", ", geenPlek)}.";
    }

    /// <summary>
    /// Volgorde waarin partijen gevuld worden. "Rep" (repiano, brassband) valt tussen de
    /// 1e en 2e cornetten; onbekende partijen komen achteraan.
    /// </summary>
    private static double PartijRang(string partij) => partij.ToLowerInvariant() switch
    {
        "" or "solo" => 0,
        "rep" or "repiano" => 1.5,
        _ when int.TryParse(partij, out var nummer) => nummer,
        _ => 100,
    };

    [RelayCommand]
    private async Task OpslaanAsync()
    {
        var gebruikt = Stoelen.Select(s => s.Instrument).Concat(Musici.Select(m => m.Instrument)).ToHashSet();
        var opstelling = new Opstelling
        {
            Naam = OpstellingNaam.Trim(),
            Musici = Musici.Select(m => m.Model).ToList(),
            Stoelen = Stoelen.Select(s => s.NaarModel()).ToList(),
            EigenInstrumenten = Catalogus.Eigen.Where(gebruikt.Contains).ToList(),
        };

        try
        {
            await _opslag.OpslaanAsync(opstelling);
            await VerversOpgeslagenAsync();
            GeselecteerdeOpstelling = opstelling.Naam;
            StatusMessage = $"Opstelling \"{opstelling.Naam}\" opgeslagen.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Opslaan mislukt: {ex.Message}";
        }
    }

    private bool KanOpenen() => GeselecteerdeOpstelling is not null;

    [RelayCommand(CanExecute = nameof(KanOpenen))]
    private async Task OpenenAsync()
    {
        if (GeselecteerdeOpstelling is not { } naam)
        {
            return;
        }

        try
        {
            if (await _opslag.LaadAsync(naam) is not { } opstelling)
            {
                StatusMessage = $"Opstelling \"{naam}\" bestaat niet (meer).";
                return;
            }

            // Eigen instrumenten uit de opstelling die hier nog niet bekend zijn, overnemen.
            var nieuw = opstelling.EigenInstrumenten.Where(i => !Catalogus.Bestaat(i.Id)).ToList();
            foreach (var instrument in nieuw)
            {
                Catalogus.VoegToe(instrument);
            }
            if (nieuw.Count > 0)
            {
                await _opslag.OpslaanEigenInstrumentenAsync(Catalogus.Eigen);
            }

            Musici.Clear();
            foreach (var musicus in opstelling.Musici)
            {
                VoegMusicusToe(musicus);
            }
            ZetStoelen(opstelling.Stoelen);
            OpstellingNaam = opstelling.Naam;
            StatusMessage = $"Opstelling \"{opstelling.Naam}\" geopend.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Openen mislukt: {ex.Message}";
        }
    }

    private async Task VerversOpgeslagenAsync()
    {
        var namen = await _opslag.NamenAsync();
        OpgeslagenOpstellingen.Clear();
        foreach (var naam in namen)
        {
            OpgeslagenOpstellingen.Add(naam);
        }
    }

    private void ZetStoelen(IEnumerable<Stoel> stoelen)
    {
        foreach (var stoel in Stoelen)
        {
            stoel.PropertyChanged -= OnStoelPropertyChanged;
        }
        Stoelen.Clear();
        foreach (var stoel in stoelen)
        {
            VoegStoelToe(stoel);
        }
        WerkPlaatsingBij();
    }

    private void VoegStoelToe(Stoel model)
    {
        var musicus = model.MusicusId is { } id ? ZoekMusicus(id) : null;
        var stoel = new StoelViewModel(model, Catalogus.Zoek(model.InstrumentId), musicus, VerwijderStoel);
        stoel.PropertyChanged += OnStoelPropertyChanged;
        Stoelen.Add(stoel);
    }

    private void VerwijderStoel(StoelViewModel stoel)
    {
        stoel.PropertyChanged -= OnStoelPropertyChanged;
        Stoelen.Remove(stoel);
        WerkPlaatsingBij();
    }

    /// <summary>Voegt een musicus toe op de juiste plek: gesorteerd op instrument en daarna naam.</summary>
    private void VoegMusicusToe(Musicus model)
    {
        var musicus = new MusicusViewModel(model, Catalogus.Zoek(model.InstrumentId), VerwijderMusicus);
        var index = 0;
        while (index < Musici.Count && Vergelijk(Musici[index], musicus) <= 0)
        {
            index++;
        }
        Musici.Insert(index, musicus);
    }

    private int Vergelijk(MusicusViewModel a, MusicusViewModel b)
    {
        var instrument = Catalogus.Volgorde(a.Instrument).CompareTo(Catalogus.Volgorde(b.Instrument));
        return instrument != 0 ? instrument : NaamComparer.Compare(a.Naam, b.Naam);
    }

    private void VerwijderMusicus(MusicusViewModel musicus)
    {
        foreach (var stoel in Stoelen.Where(s => s.Musicus == musicus))
        {
            stoel.Musicus = null;
        }
        Musici.Remove(musicus);
        WerkPlaatsingBij();
    }

    private void OnStoelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StoelViewModel.Musicus))
        {
            WerkPlaatsingBij();
        }
    }

    private void WerkPlaatsingBij()
    {
        var geplaatst = Stoelen.Select(s => s.Musicus).OfType<MusicusViewModel>().ToHashSet();
        foreach (var musicus in Musici)
        {
            musicus.IsGeplaatst = geplaatst.Contains(musicus);
        }

        var bezet = Stoelen.Count(s => s.Musicus is not null);
        PlaatsingSamenvatting =
            $"{geplaatst.Count} van {Musici.Count} musici geplaatst · {bezet} van {Stoelen.Count} stoelen bezet";
    }
}
