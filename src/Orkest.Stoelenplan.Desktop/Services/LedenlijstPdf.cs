using Orkest.Stoelenplan.Desktop.ViewModels;
using SkiaSharp;

namespace Orkest.Stoelenplan.Desktop.Services;

/// <summary>
/// Zet de ledenlijst (nummer, instrument, stem en naam) op A4-pagina's (staand) in een PDF,
/// in een monospace lettertype zodat de kolommen netjes onder elkaar staan.
/// </summary>
public static class LedenlijstPdf
{
    // Maten in PDF-punten (1/72 inch).
    private const float PaginaBreedte = 595;
    private const float PaginaHoogte = 842;
    private const float Marge = 50;
    private const float TitelGrootte = 18;
    private const float TekstGrootte = 10;
    private const float VoetGrootte = 9;
    private const float Regelafstand = TekstGrootte * 1.4f;

    // Langere instrumentnamen (bv. "Mallets (xylofoon, marimba, …)") worden ingekort.
    private const int MaxInstrumentBreedte = 20;

    private const string Kolomafstand = "  ";

    // Eerste lettertype dat op deze computer bestaat én monospace is, wint.
    private static readonly string[] MonoLettertypen =
        ["DejaVu Sans Mono", "Liberation Mono", "Noto Sans Mono", "Consolas", "Menlo", "Courier New", "monospace"];

    public static async Task SchrijfAsync(Stream doel, IReadOnlyList<LedenlijstRegel> regels, string titel)
    {
        using var buffer = new MemoryStream();
        var metadata = new SKDocumentPdfMetadata
        {
            Title = $"Ledenlijst {titel}".Trim(),
            Creator = "Orkest stoelenplan",
            Creation = DateTime.Now,
        };

        using var mono = ZoekMonoLettertype();
        using var tekstFont = new SKFont(mono, TekstGrootte);
        using var kopFont = new SKFont(SKTypeface.FromFamilyName(mono.FamilyName, SKFontStyle.Bold), TekstGrootte);
        using var titelFont = new SKFont(SKTypeface.FromFamilyName(mono.FamilyName, SKFontStyle.Bold), TitelGrootte);
        using var voetFont = new SKFont(mono, VoetGrootte);
        using var zwart = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        using var grijs = new SKPaint { Color = new SKColor(0x66, 0x66, 0x66), IsAntialias = true };
        using var lijn = new SKPaint { Color = new SKColor(0xBB, 0xBB, 0xBB), StrokeWidth = 0.5f, IsAntialias = true };
        using var streep = new SKPaint { Color = new SKColor(0xF0, 0xF0, 0xF0) };

        // Kolommen in tekens; breed genoeg voor de langste waarde, maar binnen de pagina.
        var tekenBreedte = Breedte("M", tekstFont);
        var maxTekens = (int)((PaginaBreedte - 2 * Marge) / tekenBreedte);
        var nrBreedte = Math.Max(2, regels.Count.ToString().Length);
        var stemBreedte = Math.Max("Stem".Length, regels.Select(r => r.Stem.Length).DefaultIfEmpty().Max());
        var instrumentBreedte = Math.Max("Instrument".Length, regels.Select(r => r.Instrument.Naam.Length).DefaultIfEmpty().Max());
        var vast = nrBreedte + stemBreedte + 3 * Kolomafstand.Length;
        instrumentBreedte = Math.Min(instrumentBreedte, MaxInstrumentBreedte);
        var naamBreedte = maxTekens - vast - instrumentBreedte;

        string Regel(string nr, string instrument, string stem, string naam) =>
            (nr.PadLeft(nrBreedte) + Kolomafstand + Pas(instrument, instrumentBreedte) + Kolomafstand
             + Pas(stem, stemBreedte) + Kolomafstand + Pas(naam, naamBreedte)).TrimEnd();

        var kop = Regel("#", "Instrument", "Stem", "Naam");
        var bovenTabel = Marge + TitelGrootte + 24;
        var onderTabel = PaginaHoogte - Marge - VoetGrootte - 12;
        var regelsPerPagina = Math.Max(1, (int)((onderTabel - bovenTabel) / Regelafstand) - 1);
        var paginas = Math.Max(1, (regels.Count + regelsPerPagina - 1) / regelsPerPagina);
        var datum = DateTime.Now.ToString("d-M-yyyy");

        using (var document = SKDocument.CreatePdf(buffer, metadata))
        {
            for (var pagina = 0; pagina < paginas; pagina++)
            {
                var canvas = document.BeginPage(PaginaBreedte, PaginaHoogte);
                canvas.DrawText(metadata.Title, Marge, Marge + TitelGrootte, titelFont, zwart);

                var y = bovenTabel;
                canvas.DrawText(kop, Marge, y, kopFont, zwart);
                canvas.DrawLine(Marge, y + 5, PaginaBreedte - Marge, y + 5, lijn);

                var eerste = pagina * regelsPerPagina;
                for (var i = eerste; i < Math.Min(regels.Count, eerste + regelsPerPagina); i++)
                {
                    y += Regelafstand;
                    if (i % 2 == 1)
                    {
                        // Om en om een lichte achtergrond, zodat je makkelijk een regel volgt.
                        canvas.DrawRect(Marge - 4, y - TekstGrootte - 2, PaginaBreedte - 2 * Marge + 8, Regelafstand, streep);
                    }
                    var r = regels[i];
                    canvas.DrawText(Regel((i + 1).ToString(), r.Instrument.Naam, r.Stem, r.Naam), Marge, y, tekstFont, zwart);
                }

                if (regels.Count == 0)
                {
                    canvas.DrawText("Er zijn nog geen musici toegevoegd.", Marge, y + Regelafstand, tekstFont, grijs);
                }

                var voet = $"{regels.Count} musici · {datum}";
                canvas.DrawText(voet, Marge, PaginaHoogte - Marge, voetFont, grijs);
                var paginaTekst = $"Pagina {pagina + 1} van {paginas}";
                canvas.DrawText(paginaTekst, PaginaBreedte - Marge - Breedte(paginaTekst, voetFont),
                    PaginaHoogte - Marge, voetFont, grijs);

                document.EndPage();
            }
            document.Close();
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(doel);
    }

    /// <summary>Vult aan met spaties tot <paramref name="breedte"/>, of kort in met "…" als het te lang is.</summary>
    private static string Pas(string tekst, int breedte)
        => tekst.Length <= breedte ? tekst.PadRight(breedte) : tekst[..(breedte - 1)] + "…";

    private static float Breedte(string tekst, SKFont font)
    {
        using var paint = new SKPaint { Typeface = font.Typeface, TextSize = font.Size };
        return paint.MeasureText(tekst);
    }

    private static SKTypeface ZoekMonoLettertype()
    {
        foreach (var naam in MonoLettertypen)
        {
            var typeface = SKTypeface.FromFamilyName(naam);
            if (typeface is not null && typeface.IsFixedPitch)
            {
                return typeface;
            }
            typeface?.Dispose();
        }
        return SKTypeface.FromFamilyName("Courier New") ?? SKTypeface.Default;
    }
}
