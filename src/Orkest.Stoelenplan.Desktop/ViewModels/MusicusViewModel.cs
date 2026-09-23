using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Orkest.Stoelenplan.Desktop.Services;
using Orkest.Stoelenplan.Shared.Models;

namespace Orkest.Stoelenplan.Desktop.ViewModels;

public partial class MusicusViewModel(Musicus model, Instrument instrument, Action<MusicusViewModel> verwijder)
    : ObservableObject
{
    public Guid Id => model.Id;
    public string Naam => model.Naam;
    public Instrument Instrument => instrument;
    public string InstrumentNaam => instrument.Naam;
    public IBrush Kleur => Kleuren.Brush(instrument.Kleur);

    /// <summary>Of deze musicus al op een stoel zit; wordt bijgehouden door het hoofdscherm.</summary>
    [ObservableProperty]
    private bool _isGeplaatst;

    public Musicus Model => model;

    [RelayCommand]
    private void Verwijderen() => verwijder(this);
}
