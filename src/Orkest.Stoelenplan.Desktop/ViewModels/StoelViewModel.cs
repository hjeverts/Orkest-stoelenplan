using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Orkest.Stoelenplan.Desktop.Services;
using Orkest.Stoelenplan.Shared.Models;

namespace Orkest.Stoelenplan.Desktop.ViewModels;

public partial class StoelViewModel : ObservableObject
{
    public const double Diameter = 60;

    private readonly Action<StoelViewModel> _verwijder;

    public StoelViewModel(Stoel model, Instrument instrument, MusicusViewModel? musicus, Action<StoelViewModel> verwijder)
    {
        Id = model.Id;
        Instrument = instrument;
        Partij = model.Partij.Trim();
        _x = model.X;
        _y = model.Y;
        _musicus = musicus;
        _verwijder = verwijder;
    }

    public Guid Id { get; }
    public Instrument Instrument { get; }
    public string Partij { get; }

    /// <summary>Middelpunt van de stoel in podiumcoördinaten.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Left))]
    private double _x;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Top))]
    private double _y;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Naam), nameof(ToolTip))]
    [NotifyCanExecuteChangedFor(nameof(LeegmakenCommand))]
    private MusicusViewModel? _musicus;

    /// <summary>Linkerbovenhoek voor Canvas.Left/Top, afgeleid van het middelpunt.</summary>
    public double Left => X - Diameter / 2;
    public double Top => Y - Diameter / 2;

    /// <summary>Wat er klein bovenin de stoel staat, bv. "Kl 2" of "Cor Solo".</summary>
    public string Label => Partij.Length == 0 ? Instrument.Afkorting : $"{Instrument.Afkorting} {Partij}";

    public string Naam => Musicus?.Naam ?? "";
    public IBrush Kleur => Kleuren.Brush(Instrument.Kleur);

    private string Omschrijving => Partij.Length == 0 ? Instrument.Naam : $"{Instrument.Naam} ({Partij})";

    public string ToolTip => Musicus is null
        ? $"{Omschrijving} – leeg"
        : $"{Musicus.Naam} – {Omschrijving}";

    public Stoel NaarModel() => new()
    {
        Id = Id,
        InstrumentId = Instrument.Id,
        Partij = Partij,
        X = X,
        Y = Y,
        MusicusId = Musicus?.Id,
    };

    [RelayCommand(CanExecute = nameof(IsBezet))]
    private void Leegmaken() => Musicus = null;

    private bool IsBezet() => Musicus is not null;

    [RelayCommand]
    private void Verwijderen() => _verwijder(this);
}
