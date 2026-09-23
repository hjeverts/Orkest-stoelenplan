using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Crescendo.Stoelenplan.Desktop.Services;
using Crescendo.Stoelenplan.Shared.Models;

namespace Crescendo.Stoelenplan.Desktop.ViewModels;

public partial class MusicusViewModel(Musicus model, Action<MusicusViewModel> verwijder) : ObservableObject
{
    public Guid Id => model.Id;
    public string Naam => model.Naam;
    public Sectie Sectie => model.Sectie;
    public string SectieNaam => Sectie.Weergavenaam();
    public IBrush Kleur => SectieKleuren.Voor(Sectie);

    /// <summary>Of deze musicus al op een stoel zit; wordt bijgehouden door het hoofdscherm.</summary>
    [ObservableProperty]
    private bool _isGeplaatst;

    public Musicus Model => model;

    [RelayCommand]
    private void Verwijderen() => verwijder(this);
}
