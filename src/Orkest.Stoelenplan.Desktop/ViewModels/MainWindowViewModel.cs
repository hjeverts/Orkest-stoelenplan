using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        ZetStoelen(StandaardOpstelling.MaakStoelen());
    }

    public ObservableCollection<StoelViewModel> Stoelen { get; } = [];
    public ObservableCollection<MusicusViewModel> Musici { get; } = [];
    public ObservableCollection<string> OpgeslagenOpstellingen { get; } = [];
    public IReadOnlyList<SectieOptie> Secties => SectieOptie.Alle;

    [ObservableProperty]
    private string _opstellingNaam = "Nieuwe opstelling";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenenCommand))]
    private string? _geselecteerdeOpstelling;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MusicusToevoegenCommand))]
    private string _nieuweMusicusNaam = "";

    [ObservableProperty]
    private SectieOptie _nieuweMusicusSectie = SectieOptie.Alle[0];

    [ObservableProperty]
    private SectieOptie _nieuweStoelSectie = SectieOptie.Alle[0];

    [ObservableProperty]
    private string _statusMessage = "Voeg musici toe en sleep ze naar een stoel. Stoelen zelf kun je ook verslepen.";

    [ObservableProperty]
    private string _plaatsingSamenvatting = "";

    public async Task InitialiseerAsync()
    {
        try
        {
            await VerversOpgeslagenAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kon opgeslagen opstellingen niet ophalen: {ex.Message}";
        }
        WerkPlaatsingBij();
    }

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

        if (musicus.Sectie != stoel.Sectie)
        {
            StatusMessage = $"Let op: {musicus.Naam} ({musicus.SectieNaam}) zit nu op een {stoel.Sectie.Weergavenaam()}-stoel.";
        }
    }

    public MusicusViewModel? ZoekMusicus(Guid id) => Musici.FirstOrDefault(m => m.Id == id);

    private bool KanMusicusToevoegen() => !string.IsNullOrWhiteSpace(NieuweMusicusNaam);

    [RelayCommand(CanExecute = nameof(KanMusicusToevoegen))]
    private void MusicusToevoegen()
    {
        VoegMusicusToe(new Musicus { Naam = NieuweMusicusNaam.Trim(), Sectie = NieuweMusicusSectie.Sectie });
        NieuweMusicusNaam = "";
        WerkPlaatsingBij();
    }

    [RelayCommand]
    private void StoelToevoegen()
    {
        // Nieuwe stoelen komen linksboven op het podium; schuif ze een beetje op zodat
        // meerdere nieuwe stoelen niet precies op elkaar liggen.
        var verschuiving = Stoelen.Count % 8 * 12;
        VoegStoelToe(new Stoel
        {
            Sectie = NieuweStoelSectie.Sectie,
            X = 60 + verschuiving,
            Y = 60 + verschuiving,
        });
    }

    [RelayCommand]
    private void Standaardopstelling()
    {
        ZetStoelen(StandaardOpstelling.MaakStoelen());
        StatusMessage = "Standaardopstelling neergezet; alle stoelen zijn weer leeg.";
    }

    /// <summary>
    /// Zet iedereen die nog niet zit op een lege stoel van de eigen sectie, van voor
    /// (dichtbij de dirigent) naar achter, in de volgorde van de musicilijst.
    /// </summary>
    [RelayCommand]
    private void AutomatischIndelen()
    {
        var vrijeStoelen = Stoelen
            .Where(s => s.Musicus is null)
            .OrderBy(s => Math.Pow(s.X - Opstelling.DirigentX, 2) + Math.Pow(s.Y - Opstelling.DirigentY, 2))
            .ToList();

        var geenPlek = new List<string>();
        foreach (var musicus in Musici.Where(m => !m.IsGeplaatst).ToList())
        {
            var stoel = vrijeStoelen.FirstOrDefault(s => s.Sectie == musicus.Sectie);
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
            : $"Geen vrije stoel in de eigen sectie voor: {string.Join(", ", geenPlek)}.";
    }

    [RelayCommand]
    private async Task OpslaanAsync()
    {
        var opstelling = new Opstelling
        {
            Naam = OpstellingNaam.Trim(),
            Musici = Musici.Select(m => m.Model).ToList(),
            Stoelen = Stoelen.Select(s => s.NaarModel()).ToList(),
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
        var stoel = new StoelViewModel(model, musicus, VerwijderStoel);
        stoel.PropertyChanged += OnStoelPropertyChanged;
        Stoelen.Add(stoel);
    }

    private void VerwijderStoel(StoelViewModel stoel)
    {
        stoel.PropertyChanged -= OnStoelPropertyChanged;
        Stoelen.Remove(stoel);
        WerkPlaatsingBij();
    }

    /// <summary>Voegt een musicus toe op de juiste plek: gesorteerd op sectie en daarna naam.</summary>
    private void VoegMusicusToe(Musicus model)
    {
        var musicus = new MusicusViewModel(model, VerwijderMusicus);
        var index = 0;
        while (index < Musici.Count && Vergelijk(Musici[index], musicus) <= 0)
        {
            index++;
        }
        Musici.Insert(index, musicus);
    }

    private static int Vergelijk(MusicusViewModel a, MusicusViewModel b)
    {
        var sectie = a.Sectie.CompareTo(b.Sectie);
        return sectie != 0 ? sectie : NaamComparer.Compare(a.Naam, b.Naam);
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
