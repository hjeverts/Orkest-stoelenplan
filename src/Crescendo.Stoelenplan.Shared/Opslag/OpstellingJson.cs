using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crescendo.Stoelenplan.Shared.Opslag;

public static class OpstellingJson
{
    /// <summary>
    /// Gedeelde JSON-instellingen voor bestanden én API, zodat beide hetzelfde formaat
    /// gebruiken (camelCase, secties als leesbare tekst in plaats van getallen).
    /// </summary>
    public static readonly JsonSerializerOptions Opties = MaakOpties();

    public static void Configureer(JsonSerializerOptions opties)
    {
        opties.WriteIndented = true;
        opties.Converters.Add(new JsonStringEnumConverter());
    }

    private static JsonSerializerOptions MaakOpties()
    {
        var opties = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Configureer(opties);
        return opties;
    }
}
