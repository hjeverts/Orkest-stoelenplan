using System.Text.Json;
using Crescendo.Stoelenplan.Shared.Models;

namespace Crescendo.Stoelenplan.Shared.Opslag;

/// <summary>Bewaart elke opstelling als <c>&lt;naam&gt;.json</c> in één map.</summary>
public sealed class JsonBestandOpslag(string map) : IOpstellingOpslag
{
    public Task<IReadOnlyList<string>> NamenAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> namen = Directory.Exists(map)
            ? Directory.EnumerateFiles(map, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .OfType<string>()
                .Order(StringComparer.CurrentCultureIgnoreCase)
                .ToList()
            : [];
        return Task.FromResult(namen);
    }

    public async Task<Opstelling?> LaadAsync(string naam, CancellationToken cancellationToken = default)
    {
        var pad = PadVoor(naam);
        if (!File.Exists(pad))
        {
            return null;
        }

        await using var stream = File.OpenRead(pad);
        return await JsonSerializer.DeserializeAsync<Opstelling>(stream, OpstellingJson.Opties, cancellationToken);
    }

    public async Task OpslaanAsync(Opstelling opstelling, CancellationToken cancellationToken = default)
    {
        var pad = PadVoor(opstelling.Naam);
        Directory.CreateDirectory(map);

        // Eerst naar een tijdelijk bestand, zodat een crash halverwege geen half bestand achterlaat.
        var tijdelijk = pad + ".tmp";
        await using (var stream = File.Create(tijdelijk))
        {
            await JsonSerializer.SerializeAsync(stream, opstelling, OpstellingJson.Opties, cancellationToken);
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

        return Path.Combine(map, naam + ".json");
    }
}
