using Orkest.Stoelenplan.Shared.Models;

namespace Orkest.Stoelenplan.Shared.Opslag;

/// <summary>
/// Plek waar opstellingen (op naam) en eigen instrumenten bewaard worden. De desktop-app
/// gebruikt lokaal <see cref="JsonBestandOpslag"/> of, met een backend, een HTTP-variant die
/// met de API praat.
/// </summary>
public interface IOpstellingOpslag
{
    Task<IReadOnlyList<string>> NamenAsync(CancellationToken cancellationToken = default);
    Task<Opstelling?> LaadAsync(string naam, CancellationToken cancellationToken = default);
    Task OpslaanAsync(Opstelling opstelling, CancellationToken cancellationToken = default);

    /// <summary>De instrumenten die gebruikers zelf hebben toegevoegd (de standaardinstrumenten niet).</summary>
    Task<IReadOnlyList<Instrument>> EigenInstrumentenAsync(CancellationToken cancellationToken = default);
    Task OpslaanEigenInstrumentenAsync(IReadOnlyList<Instrument> instrumenten, CancellationToken cancellationToken = default);
}
