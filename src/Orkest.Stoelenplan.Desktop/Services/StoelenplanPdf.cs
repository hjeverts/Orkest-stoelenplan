using Avalonia;
using Avalonia.Skia.Helpers;
using SkiaSharp;

namespace Orkest.Stoelenplan.Desktop.Services;

/// <summary>Zet het podium zoals het op het scherm staat op een A4-pagina (liggend) in een PDF.</summary>
public static class StoelenplanPdf
{
    // Maten in PDF-punten (1/72 inch).
    private const float PaginaBreedte = 842;
    private const float PaginaHoogte = 595;
    private const float Marge = 36;
    private const float TitelGrootte = 20;
    private const float VoetGrootte = 10;

    public static async Task SchrijfAsync(Stream doel, Visual podium, string titel, string onderschrift)
    {
        // Eerst het podium als SKPicture opnemen: zo blijft alles vector (scherpe tekst)
        // en kunnen we het daarna vrij schalen en verschuiven op de pagina.
        var grootte = podium.Bounds.Size;
        using var opname = new SKPictureRecorder();
        var opnameCanvas = opname.BeginRecording(SKRect.Create((float)grootte.Width, (float)grootte.Height));
        await DrawingContextHelper.RenderAsync(opnameCanvas, podium);
        using var tekening = opname.EndRecording();

        using var buffer = new MemoryStream();
        var metadata = new SKDocumentPdfMetadata
        {
            Title = titel,
            Creator = "Orkest stoelenplan",
            Creation = DateTime.Now,
        };
        using (var document = SKDocument.CreatePdf(buffer, metadata))
        {
            var canvas = document.BeginPage(PaginaBreedte, PaginaHoogte);

            using var tekst = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            using var titelFont = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyle.Bold), TitelGrootte);
            using var voetFont = new SKFont(SKTypeface.Default, VoetGrootte);
            canvas.DrawText(titel, Marge, Marge + TitelGrootte, titelFont, tekst);

            // Podium zo groot mogelijk, horizontaal gecentreerd, tussen titel en voetregel.
            var ruimte = new SKRect(Marge, Marge + TitelGrootte + 14, PaginaBreedte - Marge, PaginaHoogte - Marge - VoetGrootte - 10);
            var schaal = (float)Math.Min(ruimte.Width / grootte.Width, ruimte.Height / grootte.Height);
            var plaatsing = SKMatrix.CreateScale(schaal, schaal)
                .PostConcat(SKMatrix.CreateTranslation(ruimte.Left + (ruimte.Width - (float)grootte.Width * schaal) / 2, ruimte.Top));
            canvas.DrawPicture(tekening, ref plaatsing);

            tekst.Color = new SKColor(0x55, 0x55, 0x55);
            canvas.DrawText($"{onderschrift} · {DateTime.Now:d-M-yyyy}", Marge, PaginaHoogte - Marge, voetFont, tekst);

            document.EndPage();
            document.Close();
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(doel);
    }
}
