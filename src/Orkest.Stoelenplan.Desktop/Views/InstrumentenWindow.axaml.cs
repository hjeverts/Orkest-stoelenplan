using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Orkest.Stoelenplan.Desktop.Views;

public partial class InstrumentenWindow : Window
{
    public InstrumentenWindow()
    {
        InitializeComponent();
    }

    private void Sluiten_Click(object? sender, RoutedEventArgs e) => Close();
}
