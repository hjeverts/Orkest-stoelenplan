using Crescendo.Stoelenplan.Shared.Models;
using Crescendo.Stoelenplan.Shared.Opslag;

// Voorlopig een eenvoudige API die opstellingen als JSON-bestanden op de server bewaart.
// Let op: er zit nog geen authenticatie op — alleen draaien op een vertrouwd netwerk.
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o => OpstellingJson.Configureer(o.SerializerOptions));

var opslagMap = Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["Opslag:Map"] ?? "data/opstellingen");
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

app.Run();
