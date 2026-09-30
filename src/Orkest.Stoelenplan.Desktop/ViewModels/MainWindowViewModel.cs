using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
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

    /// <summary>Stemmen met cijfers op getalwaarde ("2" vóór "10"), zonder stem eerst.</summary>
    private static readonly StringComparer StemComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("nl-NL"), CompareOptions.IgnoreCase | CompareOptions.NumericOrdering);

    /// <summary>Hoeveel acties je terug kunt met "Ongedaan maken".</summary>
    private const int MaxOngedaan = 10;

    private readonly IOpstellingOpslag _opslag;

    /// <summary>De stand van vóór elke actie, de laatste actie achteraan.</summary>
    private readonly LinkedList<(string Actie, Opstelling Stand)> _geschiedenis = new();

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
        if (stoel.Musicus == musicus)
        {
            return;
        }

        Onthoud($"{musicus.Naam} plaatsen");
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
        Onthoud($"{NieuweMusicusNaam.Trim()} toevoegen");
        VoegMusicusToe(new Musicus { Naam = NieuweMusicusNaam.Trim(), InstrumentId = NieuweMusicusInstrument!.Id });
        NieuweMusicusNaam = "";
        WerkPlaatsingBij();
    }

    private bool KanStoelToevoegen() => NieuweStoelInstrument is not null;

    [RelayCommand(CanExecute = nameof(KanStoelToevoegen))]
    private void StoelToevoegen()
    {
        Onthoud("stoel toevoegen");
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
        Onthoud("standaardopstelling");
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
        var stand = Momentopname();
        var ingedeeld = 0;
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
            ingedeeld++;
        }

        if (ingedeeld > 0)
        {
            Onthoud("automatisch indelen", stand);
        }

        StatusMessage = geenPlek.Count == 0
            ? "Iedereen is ingedeeld."
            : $"Geen vrije stoel voor het eigen instrument van: {string.Join(", ", geenPlek)}.";
    }

    /// <summary>
    /// Maakt de tussenruimte tussen de stoelen netter: rijen komen op één boog (of rechte lijn)
    /// met gelijke afstanden; volgorde en buitenste stoelen van elke rij blijven gelijk.
    /// </summary>
    [RelayCommand]
    private void StoelenUitlijnen()
    {
        Onthoud("stoelen uitlijnen");
        var nieuw = Shared.Models.StoelenUitlijnen.Lijn(Stoelen.Select(s => (s.X, s.Y)).ToList());
        for (var i = 0; i < Stoelen.Count; i++)
        {
            (Stoelen[i].X, Stoelen[i].Y) = nieuw[i];
        }
        StatusMessage = "Stoelen uitgelijnd. Niet tevreden? Gebruik Ongedaan maken.";
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

    /// <summary>De opstelling zoals die nu op het scherm staat, inclusief de gebruikte eigen instrumenten.</summary>
    private Opstelling HuidigeOpstelling()
    {
        var gebruikt = Stoelen.Select(s => s.Instrument).Concat(Musici.Select(m => m.Instrument)).ToHashSet();
        return new Opstelling
        {
            Naam = OpstellingNaam.Trim(),
            Musici = Musici.Select(m => m.Model).ToList(),
            Stoelen = Stoelen.Select(s => s.NaarModel()).ToList(),
            EigenInstrumenten = Catalogus.Eigen.Where(gebruikt.Contains).ToList(),
        };
    }

    [RelayCommand]
    private async Task OpslaanAsync()
    {
        var opstelling = HuidigeOpstelling();

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

            Onthoud($"\"{opstelling.Naam}\" openen");
            await ToonAsync(opstelling);
            StatusMessage = $"Opstelling \"{opstelling.Naam}\" geopend.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Openen mislukt: {ex.Message}";
        }
    }

    /// <summary>Zet een opstelling op het scherm en neemt onbekende eigen instrumenten eruit over.</summary>
    private async Task ToonAsync(Opstelling opstelling)
    {
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
    }

    /// <summary>Schrijft de opstelling op het scherm als JSON naar <paramref name="doel"/>, bv. een gekozen bestand.</summary>
    public async Task ExporteerAsync(Stream doel, string bestandsnaam)
    {
        try
        {
            await JsonSerializer.SerializeAsync(doel, HuidigeOpstelling(), OpstellingJson.Opties);
            StatusMessage = $"Opstelling geëxporteerd naar \"{bestandsnaam}\".";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Exporteren mislukt: {ex.Message}";
        }
    }

    /// <summary>
    /// Leest een geëxporteerde opstelling in. Zonder naam in het bestand wordt de bestandsnaam
    /// gebruikt. Geeft <c>null</c> (met een melding in de statusbalk) als het bestand onbruikbaar is.
    /// </summary>
    public async Task<Opstelling?> LeesImportAsync(Stream bron, string bestandsnaam)
    {
        try
        {
            var opstelling = await JsonSerializer.DeserializeAsync<Opstelling>(bron, OpstellingJson.Opties);
            if (opstelling is null || (opstelling.Stoelen.Count == 0 && opstelling.Musici.Count == 0))
            {
                StatusMessage = $"\"{bestandsnaam}\" bevat geen opstelling.";
                return null;
            }

            opstelling.Naam = string.IsNullOrWhiteSpace(opstelling.Naam)
                ? Path.GetFileNameWithoutExtension(bestandsnaam)
                : opstelling.Naam.Trim();
            return opstelling;
        }
        catch (JsonException)
        {
            StatusMessage = $"\"{bestandsnaam}\" is geen geldig stoelenplan-bestand.";
            return null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Importeren mislukt: {ex.Message}";
            return null;
        }
    }

    public bool BestaatAl(string naam) => OpgeslagenOpstellingen.Contains(naam.Trim(), StringComparer.OrdinalIgnoreCase);

    /// <summary>Eerste vrije variant van <paramref name="naam"/>: "Naam (2)", "Naam (3)", …</summary>
    public string VrijeNaam(string naam)
    {
        var nummer = 2;
        while (BestaatAl($"{naam} ({nummer})"))
        {
            nummer++;
        }
        return $"{naam} ({nummer})";
    }

    /// <summary>Slaat een ingelezen opstelling op in de eigen lijst en zet hem op het scherm.</summary>
    public async Task ImporteerAsync(Opstelling opstelling)
    {
        try
        {
            await _opslag.OpslaanAsync(opstelling);
            Onthoud($"\"{opstelling.Naam}\" importeren");
            await ToonAsync(opstelling);
            await VerversOpgeslagenAsync();
            GeselecteerdeOpstelling = opstelling.Naam;
            StatusMessage = $"Opstelling \"{opstelling.Naam}\" geïmporteerd.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Importeren mislukt: {ex.Message}";
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
        var stoel = new StoelViewModel(model, Catalogus.Zoek(model.InstrumentId), musicus, MaakStoelLeeg, VerwijderStoel);
        stoel.PropertyChanged += OnStoelPropertyChanged;
        Stoelen.Add(stoel);
    }

    private void MaakStoelLeeg(StoelViewModel stoel)
    {
        Onthoud($"stoel leegmaken ({stoel.Musicus?.Naam})");
        stoel.Musicus = null;
    }

    private void VerwijderStoel(StoelViewModel stoel)
    {
        Onthoud("stoel verwijderen");
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

    /// <summary>
    /// Alle musici voor de ledenlijst, gesorteerd op instrument, stem en naam. De stem is de
    /// partij van de stoel waar de musicus op zit (leeg als hij nog niet geplaatst is).
    /// </summary>
    public IReadOnlyList<LedenlijstRegel> Ledenlijst()
    {
        var stemPerMusicus = Stoelen
            .Where(s => s.Musicus is not null)
            .ToDictionary(s => s.Musicus!, s => s.Partij);

        return Musici
            .Select(m => new LedenlijstRegel(m.Naam, m.Instrument, stemPerMusicus.GetValueOrDefault(m, "")))
            .OrderBy(r => Catalogus.Volgorde(r.Instrument))
            .ThenBy(r => r.Stem, StemComparer)
            .ThenBy(r => r.Naam, NaamComparer)
            .ToList();
    }

    private int Vergelijk(MusicusViewModel a, MusicusViewModel b)
    {
        var instrument = Catalogus.Volgorde(a.Instrument).CompareTo(Catalogus.Volgorde(b.Instrument));
        return instrument != 0 ? instrument : NaamComparer.Compare(a.Naam, b.Naam);
    }

    private void VerwijderMusicus(MusicusViewModel musicus)
    {
        Onthoud($"{musicus.Naam} verwijderen");
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

    // --- Ongedaan maken -----------------------------------------------------

    /// <summary>
    /// Bewaart de huidige stand vlak vóór een actie, zodat "Ongedaan maken" ernaar terug kan.
    /// Alleen de laatste <see cref="MaxOngedaan"/> acties worden onthouden.
    /// </summary>
    private void Onthoud(string actie) => Onthoud(actie, Momentopname());

    private void Onthoud(string actie, Opstelling stand)
    {
        _geschiedenis.AddLast((actie, stand));
        if (_geschiedenis.Count > MaxOngedaan)
        {
            _geschiedenis.RemoveFirst();
        }
        GeschiedenisGewijzigd();
    }

    /// <summary>
    /// Een stoel is met de muis versleept. Het plan staat al op de nieuwe plek, dus de
    /// bewaarde stand krijgt de oude plek van de stoel.
    /// </summary>
    public void StoelVerplaatst(StoelViewModel stoel, double vanX, double vanY)
    {
        var stand = Momentopname();
        var model = stand.Stoelen.First(s => s.Id == stoel.Id);
        (model.X, model.Y) = (vanX, vanY);
        Onthoud("stoel verplaatsen", stand);
    }

    public bool KanOngedaanMaken => _geschiedenis.Count > 0;

    public string OngedaanMakenTip => _geschiedenis.Last is { } laatste
        ? $"Maak ongedaan: {laatste.Value.Actie} (Ctrl+Z)"
        : "Er is niets om ongedaan te maken";

    [RelayCommand(CanExecute = nameof(KanOngedaanMaken))]
    private void OngedaanMaken()
    {
        if (_geschiedenis.Last is not { } laatste)
        {
            return;
        }

        _geschiedenis.RemoveLast();
        var (actie, stand) = laatste.Value;
        Musici.Clear();
        foreach (var musicus in stand.Musici)
        {
            VoegMusicusToe(musicus);
        }
        ZetStoelen(stand.Stoelen);
        OpstellingNaam = stand.Naam;
        GeschiedenisGewijzigd();
        StatusMessage = $"Ongedaan gemaakt: {actie}.";
    }

    private void GeschiedenisGewijzigd()
    {
        OngedaanMakenCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(OngedaanMakenTip));
    }

    /// <summary>Een losse kopie van wat er nu op het scherm staat (inclusief de naam, niet ingekort).</summary>
    private Opstelling Momentopname() => new()
    {
        Naam = OpstellingNaam,
        Musici = Musici.Select(m => new Musicus { Id = m.Id, Naam = m.Model.Naam, InstrumentId = m.Model.InstrumentId }).ToList(),
        Stoelen = Stoelen.Select(s => s.NaarModel()).ToList(),
    };

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
