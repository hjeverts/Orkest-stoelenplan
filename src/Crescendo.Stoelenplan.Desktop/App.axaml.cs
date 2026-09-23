using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Crescendo.Stoelenplan.Desktop.Services;
using Crescendo.Stoelenplan.Desktop.ViewModels;
using Crescendo.Stoelenplan.Desktop.Views;
using Crescendo.Stoelenplan.Shared.Opslag;

namespace Crescendo.Stoelenplan.Desktop;

public partial class App : Application
{
    /// <summary>
    /// Als deze omgevingsvariabele is gezet (bv. <c>http://localhost:5731/</c>) praat de app
    /// met de API in plaats van lokaal op te slaan.
    /// </summary>
    private const string ApiUrlVariabele = "CRESCENDO_API_URL";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = new MainWindowViewModel(MaakOpslag());
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };
            desktop.MainWindow.Opened += async (_, _) => await viewModel.InitialiseerAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IOpstellingOpslag MaakOpslag()
    {
        var apiUrl = Environment.GetEnvironmentVariable(ApiUrlVariabele);
        if (!string.IsNullOrWhiteSpace(apiUrl))
        {
            // BaseAddress moet op een '/' eindigen, anders valt het laatste pad-deel weg.
            return new ApiOpslag(new HttpClient { BaseAddress = new Uri(apiUrl.TrimEnd('/') + "/") });
        }

        var map = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Crescendo.Stoelenplan", "opstellingen");
        return new JsonBestandOpslag(map);
    }
}
