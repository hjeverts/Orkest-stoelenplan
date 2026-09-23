using System.Text.Json;
using Orkest.Stoelenplan.Shared.Models;

namespace Orkest.Stoelenplan.Shared.Opslag;

/// <summary>
/// Bewaart elke opstelling als <c>opstellingen/&lt;naam&gt;.json</c> en de eigen instrumenten
/// in <c>instrumenten.json</c>, allebei onder <paramref name="basisMap"/>.
/// </summary>
public sealed class JsonBestandOpslag(string basisMap) : IOpstellingOpslag
{
    private readonly string _opstellingenMap = Path.Combine(basisMap, "opstellingen");
    private readonly string _instrumentenPad = Path.Combine(basisMap, "instrumenten.json");

    public Task<IReadOnlyList<string>> NamenAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> namen = Directory.Exists(_opstellingenMap)
            ? Directory.EnumerateFiles(_opstellingenMap, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .OfType<string>()
                .Order(StringComparer.CurrentCultureIgnoreCase)
                .ToList()
            : [];
        return Task.FromResult(namen);
    }

    public Task<Opstelling?> LaadAsync(string naam, CancellationToken cancellationToken = default)
        => LeesAsync<Opstelling>(PadVoor(naam), cancellationToken);

    public Task OpslaanAsync(Opstelling opstelling, CancellationToken cancellationToken = default)
        => SchrijfAsync(PadVoor(opstelling.Naam), opstelling, cancellationToken);

    public async Task<IReadOnlyList<Instrument>> EigenInstrumentenAsync(CancellationToken cancellationToken = default)
        => await LeesAsync<List<Instrument>>(_instrumentenPad, cancellationToken) ?? [];

    public Task OpslaanEigenInstrumentenAsync(IReadOnlyList<Instrument> instrumenten, CancellationToken cancellationToken = default)
        => SchrijfAsync(_instrumentenPad, instrumenten, cancellationToken);

    private static async Task<T?> LeesAsync<T>(string pad, CancellationToken cancellationToken)
    {
        if (!File.Exists(pad))
        {
            return default;
        }

        await using var stream = File.OpenRead(pad);
        return await JsonSerializer.DeserializeAsync<T>(stream, OpstellingJson.Opties, cancellationToken);
    }

    private static async Task SchrijfAsync<T>(string pad, T waarde, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(pad)!);

        // Eerst naar een tijdelijk bestand, zodat een crash halverwege geen half bestand achterlaat.
        var tijdelijk = pad + ".tmp";
        await using (var stream = File.Create(tijdelijk))
        {
            await JsonSerializer.SerializeAsync(stream, waarde, OpstellingJson.Opties, cancellationToken);
        }
        File.Move(tijdelijk, pad, overwrite: true);
    }

    /// <summary>
    /// Zet een opstellingsnaam om naar een bestandspad. Namen komen via de API ook van
    /// buitenaf, dus alles wat buiten de map zou kunnen wijzen wordt geweigerd.
    /// </summary>
    private string PadVoor(string naam)
    {
        naam = naam.Trim();
        if (naam.Length == 0
            || naam.StartsWith('.')
            || naam.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || naam.Contains('/')
            || naam.Contains('\\'))
        {
            throw new ArgumentException($"Ongeldige naam voor een opstelling: \"{naam}\".");
        }

        return Path.Combine(_opstellingenMap, naam + ".json");
    }
}
