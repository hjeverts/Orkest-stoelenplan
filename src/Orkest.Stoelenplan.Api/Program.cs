using Orkest.Stoelenplan.Shared.Models;
using Orkest.Stoelenplan.Shared.Opslag;

// Voorlopig een eenvoudige API die opstellingen als JSON-bestanden op de server bewaart.
// Let op: er zit nog geen authenticatie op — alleen draaien op een vertrouwd netwerk.
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o => OpstellingJson.Configureer(o.SerializerOptions));

var opslagMap = Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["Opslag:Map"] ?? "data");
builder.Services.AddSingleton<IOpstellingOpslag>(new JsonBestandOpslag(opslagMap));

var app = builder.Build();

var opstellingen = app.MapGroup("/opstellingen");

opstellingen.MapGet("/", (IOpstellingOpslag opslag, CancellationToken ct) => opslag.NamenAsync(ct));

opstellingen.MapGet("/{naam}", async (string naam, IOpstellingOpslag opslag, CancellationToken ct) =>
{
    try
    {
        return await opslag.LaadAsync(naam, ct) is { } opstelling
            ? Results.Ok(opstelling)
            : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

opstellingen.MapPut("/{naam}", async (string naam, Opstelling opstelling, IOpstellingOpslag opslag, CancellationToken ct) =>
{
    opstelling.Naam = naam;
    try
    {
        await opslag.OpslaanAsync(opstelling, ct);
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Eigen instrumenten (de standaardinstrumenten zitten in de app zelf).
app.MapGet("/instrumenten", (IOpstellingOpslag opslag, CancellationToken ct) => opslag.EigenInstrumentenAsync(ct));

app.MapPut("/instrumenten", async (List<Instrument> instrumenten, IOpstellingOpslag opslag, CancellationToken ct) =>
{
    if (instrumenten.Any(i => string.IsNullOrWhiteSpace(i.Id) || string.IsNullOrWhiteSpace(i.Naam)))
    {
        return Results.BadRequest("Elk instrument heeft een id en een naam nodig.");
    }

    await opslag.OpslaanEigenInstrumentenAsync(instrumenten, ct);
    return Results.NoContent();
});

app.Run();
