using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Orkest.Stoelenplan.Desktop.Views;

public enum NaamBestaatKeuze
{
    Annuleren,
    Overschrijven,
    NieuweNaam,
}

/// <summary>Vraagt wat er moet gebeuren als een geïmporteerde opstelling een bestaande naam heeft.</summary>
public partial class NaamBestaatWindow : Window
{
    public NaamBestaatWindow() : this("", "")
    {
    }

    public NaamBestaatWindow(string naam, string nieuweNaam)
    {
        InitializeComponent();
        Vraag.Text = $"Er bestaat al een opstelling \"{naam}\". Wil je die overschrijven, "
            + $"of de geïmporteerde opstelling opslaan als \"{nieuweNaam}\"?";
        NieuweNaamKnop.Content = $"Opslaan als \"{nieuweNaam}\"";
    }

    private void Annuleren_Click(object? sender, RoutedEventArgs e) => Close(NaamBestaatKeuze.Annuleren);

    private void Overschrijven_Click(object? sender, RoutedEventArgs e) => Close(NaamBestaatKeuze.Overschrijven);

    private void NieuweNaam_Click(object? sender, RoutedEventArgs e) => Close(NaamBestaatKeuze.NieuweNaam);
}
