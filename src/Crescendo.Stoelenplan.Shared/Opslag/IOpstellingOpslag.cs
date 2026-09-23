using Crescendo.Stoelenplan.Shared.Models;

namespace Crescendo.Stoelenplan.Shared.Opslag;

/// <summary>
/// Plek waar opstellingen bewaard worden, op naam. De desktop-app gebruikt lokaal
/// <see cref="JsonBestandOpslag"/> of, met een backend, een HTTP-variant die met de API praat.
/// </summary>
public interface IOpstellingOpslag
{
    Task<IReadOnlyList<string>> NamenAsync(CancellationToken cancellationToken = default);
    Task<Opstelling?> LaadAsync(string naam, CancellationToken cancellationToken = default);
    Task OpslaanAsync(Opstelling opstelling, CancellationToken cancellationToken = default);
}
