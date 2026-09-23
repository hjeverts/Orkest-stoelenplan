using System.Net;
using System.Net.Http.Json;
using Orkest.Stoelenplan.Shared.Models;
using Orkest.Stoelenplan.Shared.Opslag;

namespace Orkest.Stoelenplan.Desktop.Services;

/// <summary>Bewaart opstellingen via de Orkest.Stoelenplan.Api in plaats van lokaal.</summary>
public sealed class ApiOpslag(HttpClient http) : IOpstellingOpslag
{
    public async Task<IReadOnlyList<string>> NamenAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<string>>("opstellingen", OpstellingJson.Opties, cancellationToken) ?? [];

    public async Task<Opstelling?> LaadAsync(string naam, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync(Pad(naam), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Opstelling>(OpstellingJson.Opties, cancellationToken);
    }

    public async Task OpslaanAsync(Opstelling opstelling, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync(Pad(opstelling.Naam), opstelling, OpstellingJson.Opties, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static string Pad(string naam) => "opstellingen/" + Uri.EscapeDataString(naam.Trim());
}
