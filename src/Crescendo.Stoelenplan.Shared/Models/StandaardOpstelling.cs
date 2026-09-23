namespace Crescendo.Stoelenplan.Shared.Models;

/// <summary>
/// Maakt een klassieke symfonie-opstelling: strijkers in halve cirkels rond de dirigent,
/// houtblazers daarachter in het midden en koper/slagwerk op de achterste rij.
/// </summary>
public static class StandaardOpstelling
{
    // Hoeken in graden gemeten vanaf de dirigent: 180 = helemaal links, 90 = recht
    // vooruit (achter op het podium), 0 = helemaal rechts.
    private sealed record Boog(Sectie Sectie, double Straal, double VanHoek, double TotHoek, int Aantal);

    private static readonly Boog[] Bogen =
    [
        // Rij 1
        new(Sectie.Viool1, 170, 180, 135, 2),
        new(Sectie.Viool2, 170, 135, 90, 2),
        new(Sectie.Altviool, 170, 90, 45, 2),
        new(Sectie.Cello, 170, 45, 0, 2),
        // Rij 2
        new(Sectie.Viool1, 250, 180, 135, 3),
        new(Sectie.Viool2, 250, 135, 90, 3),
        new(Sectie.Altviool, 250, 90, 45, 3),
        new(Sectie.Cello, 250, 45, 0, 3),
        // Rij 3
        new(Sectie.Viool1, 330, 180, 135, 4),
        new(Sectie.Viool2, 330, 135, 90, 4),
        new(Sectie.Altviool, 330, 90, 45, 3),
        new(Sectie.Cello, 330, 45, 0, 3),
        // Rij 4: harp links, houtblazers midden, contrabassen rechts
        new(Sectie.Harp, 410, 165, 150, 1),
        new(Sectie.Fluit, 410, 125, 107.5, 2),
        new(Sectie.Hobo, 410, 107.5, 90, 2),
        new(Sectie.Klarinet, 410, 90, 72.5, 2),
        new(Sectie.Fagot, 410, 72.5, 55, 2),
        new(Sectie.Contrabas, 410, 45, 20, 3),
        // Rij 5: koper en slagwerk
        new(Sectie.Hoorn, 490, 160, 130, 4),
        new(Sectie.Trompet, 490, 128, 104, 3),
        new(Sectie.Trombone, 490, 100, 76, 3),
        new(Sectie.Tuba, 490, 72, 64, 1),
        new(Sectie.Slagwerk, 490, 55, 30, 3),
    ];

    public static List<Stoel> MaakStoelen()
    {
        var stoelen = new List<Stoel>();
        foreach (var boog in Bogen)
        {
            var stap = (boog.TotHoek - boog.VanHoek) / boog.Aantal;
            for (var i = 0; i < boog.Aantal; i++)
            {
                var hoek = (boog.VanHoek + (i + 0.5) * stap) * Math.PI / 180;
                stoelen.Add(new Stoel
                {
                    Sectie = boog.Sectie,
                    X = Math.Round(Opstelling.DirigentX + boog.Straal * Math.Cos(hoek)),
                    Y = Math.Round(Opstelling.DirigentY - boog.Straal * Math.Sin(hoek)),
                });
            }
        }
        return stoelen;
    }
}
