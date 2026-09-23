using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Crescendo.Stoelenplan.Desktop.Services;
using Crescendo.Stoelenplan.Shared.Models;

namespace Crescendo.Stoelenplan.Desktop.ViewModels;

public partial class StoelViewModel : ObservableObject
{
    public const double Diameter = 60;

    private readonly Action<StoelViewModel> _verwijder;

    public StoelViewModel(Stoel model, MusicusViewModel? musicus, Action<StoelViewModel> verwijder)
    {
        Id = model.Id;
        Sectie = model.Sectie;
        _x = model.X;
        _y = model.Y;
        _musicus = musicus;
        _verwijder = verwijder;
    }

    public Guid Id { get; }
    public Sectie Sectie { get; }

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

    public string Afkorting => Sectie.Afkorting();
    public string Naam => Musicus?.Naam ?? "";
    public IBrush Kleur => SectieKleuren.Voor(Sectie);

    public string ToolTip => Musicus is null
        ? $"{Sectie.Weergavenaam()} (leeg)"
        : $"{Musicus.Naam} – {Sectie.Weergavenaam()}";

    public Stoel NaarModel() => new()
    {
        Id = Id,
        Sectie = Sectie,
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
