using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Orkest.Stoelenplan.Desktop.ViewModels;
using Orkest.Stoelenplan.Shared.Models;

namespace Orkest.Stoelenplan.Desktop.Views;

public partial class MainWindow : Window
{
    private const string MusicusFormaat = "application/x-orkest-stoelenplan-musicus";

    // Stoel die nu met de muis versleept wordt, en waar binnen de stoel je hem vastpakte.
    private StoelViewModel? _gesleepteStoel;
    private Point _sleepOffset;

    public MainWindow()
    {
        InitializeComponent();
        Podium.AddHandler(DragDrop.DragOverEvent, Podium_DragOver);
        Podium.AddHandler(DragDrop.DropEvent, Podium_Drop);
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    // --- Stoelen verslepen -------------------------------------------------

    private void Stoel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: StoelViewModel stoel } control)
        {
            return;
        }

        var punt = e.GetCurrentPoint(Podium);
        if (!punt.Properties.IsLeftButtonPressed)
        {
            return;
        }

        _gesleepteStoel = stoel;
        _sleepOffset = new Point(punt.Position.X - stoel.X, punt.Position.Y - stoel.Y);
        e.Pointer.Capture(control);
        e.Handled = true;
    }

    private void Stoel_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_gesleepteStoel is not { } stoel)
        {
            return;
        }

        var positie = e.GetPosition(Podium);
        const double straal = StoelViewModel.Diameter / 2;
        stoel.X = Math.Clamp(positie.X - _sleepOffset.X, straal, Opstelling.PodiumBreedte - straal);
        stoel.Y = Math.Clamp(positie.Y - _sleepOffset.Y, straal, Opstelling.PodiumHoogte - straal);
    }

    private void Stoel_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_gesleepteStoel is null)
        {
            return;
        }

        _gesleepteStoel = null;
        e.Pointer.Capture(null);
    }

    // --- Musici naar een stoel slepen ---------------------------------------

    private async void Musicus_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: MusicusViewModel musicus } control
            || !e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var data = new DataObject();
        data.Set(MusicusFormaat, musicus.Id.ToString());
        await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
    }

    private void Podium_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = ZoekDoel(e) is not null ? DragDropEffects.Move : DragDropEffects.None;
    }

    private void Podium_Drop(object? sender, DragEventArgs e)
    {
        if (ZoekDoel(e) is ({ } musicus, { } stoel))
        {
            ViewModel?.Plaats(musicus, stoel);
        }
    }

    /// <summary>De gesleepte musicus en de stoel waar de muis boven hangt, als dat allebei klopt.</summary>
    private (MusicusViewModel Musicus, StoelViewModel Stoel)? ZoekDoel(DragEventArgs e)
    {
        if (ViewModel is not { } viewModel
            || e.Data.Get(MusicusFormaat) is not string id
            || !Guid.TryParse(id, out var musicusId)
            || viewModel.ZoekMusicus(musicusId) is not { } musicus
            || (e.Source as Control)?.DataContext is not StoelViewModel stoel)
        {
            return null;
        }

        return (musicus, stoel);
    }
}
