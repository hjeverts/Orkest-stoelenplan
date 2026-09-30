namespace Orkest.Stoelenplan.Shared.Models;

/// <summary>
/// Maakt de afstanden tussen stoelen netter zonder de opstelling om te gooien. Stoelen die
/// dicht bij elkaar op (ongeveer) dezelfde boog rond de dirigent staan, vormen een rij; die
/// rij komt op één boog met gelijke tussenruimte, waarbij de buitenste stoelen en de volgorde
/// blijven zoals ze waren. Rechte rijen (bv. een bigband) gaan net zo, maar dan horizontaal.
/// Losse stoelen blijven staan, tenzij ze over een andere stoel heen liggen.
/// </summary>
public static class StoelenUitlijnen
{
    /// <summary>Doorsnede van een stoel op het podium, gelijk aan die in het plan.</summary>
    public const double Diameter = 60;

    /// <summary>Stoelen die dichter bij elkaar staan dan dit, horen bij dezelfde rij.</summary>
    private const double BuurAfstand = Diameter * 1.9;

    /// <summary>Hoeveel stoelen in één rij van de boog (of rechte lijn) mogen afwijken.</summary>
    private const double RijTolerantie = 25;

    /// <summary>Kleinste afstand tussen twee stoelen in een rij, van middelpunt tot middelpunt.</summary>
    private const double MinAfstand = Diameter + 6;

    /// <summary>Geeft voor elke stoel (in dezelfde volgorde) de nieuwe plek.</summary>
    public static IReadOnlyList<(double X, double Y)> Lijn(IReadOnlyList<(double X, double Y)> stoelen)
    {
        var plekken = stoelen.ToArray();
        var inRij = new bool[plekken.Length];

        foreach (var (rij, recht) in KiesRijen(stoelen))
        {
            if (recht)
            {
                LijnRecht(stoelen, plekken, rij);
            }
            else
            {
                LijnBoog(stoelen, plekken, rij);
            }
            foreach (var i in rij)
            {
                inRij[i] = true;
            }
        }

        SchuifLosseStoelen(plekken, inRij);
        return plekken.Select(p => (Math.Round(p.X), Math.Round(p.Y))).ToList();
    }

    /// <summary>
    /// Zoekt bogen (zelfde afstand tot de dirigent) en rechte rijen (zelfde hoogte). Aan de
    /// zijkanten van een boogopstelling staan stoelen uit verschillende bogen ook op één
    /// hoogte; daarom wint per stoel de grootste rij, en bij gelijke grootte de boog.
    /// </summary>
    private static List<(List<int> Rij, bool Recht)> KiesRijen(IReadOnlyList<(double X, double Y)> stoelen)
    {
        var kandidaten = ZoekRijen(stoelen, (a, b) => Math.Abs(Straal(a) - Straal(b)) < RijTolerantie)
            .Select(rij => (Rij: rij, Recht: false))
            .Concat(ZoekRijen(stoelen, (a, b) => Math.Abs(a.Y - b.Y) < RijTolerantie).Select(rij => (Rij: rij, Recht: true)))
            .OrderByDescending(k => k.Rij.Count)
            .ThenBy(k => k.Recht);

        var bezet = new HashSet<int>();
        var gekozen = new List<(List<int>, bool)>();
        foreach (var kandidaat in kandidaten)
        {
            if (kandidaat.Rij.Any(bezet.Contains))
            {
                continue;
            }
            gekozen.Add(kandidaat);
            bezet.UnionWith(kandidaat.Rij);
        }
        return gekozen;
    }

    /// <summary>Groepen van twee of meer stoelen die via buren (dicht bij elkaar én <paramref name="opEenLijn"/>) aan elkaar vastzitten.</summary>
    private static List<List<int>> ZoekRijen(IReadOnlyList<(double X, double Y)> stoelen,
        Func<(double X, double Y), (double X, double Y), bool> opEenLijn)
    {
        var groep = Enumerable.Range(0, stoelen.Count).ToArray();
        int Wortel(int i) => groep[i] == i ? i : groep[i] = Wortel(groep[i]);

        for (var i = 0; i < stoelen.Count; i++)
        {
            for (var j = i + 1; j < stoelen.Count; j++)
            {
                if (Afstand(stoelen[i], stoelen[j]) < BuurAfstand && opEenLijn(stoelen[i], stoelen[j]))
                {
                    groep[Wortel(i)] = Wortel(j);
                }
            }
        }

        return Enumerable.Range(0, stoelen.Count)
            .GroupBy(Wortel)
            .Select(g => g.ToList())
            .Where(g => g.Count >= 2)
            .ToList();
    }

    private static void LijnBoog(IReadOnlyList<(double X, double Y)> stoelen, (double X, double Y)[] plekken, List<int> rij)
    {
        var volgorde = rij.OrderBy(i => Hoek(stoelen[i])).ToList();
        var straal = Mediaan(rij.Select(i => Straal(stoelen[i])));
        var van = Hoek(stoelen[volgorde[0]]);
        var tot = Hoek(stoelen[volgorde[^1]]);

        // Te krap? Dan de rij rond het midden wat breder maken, maar niet voorbij de dirigent.
        var nodig = MinAfstand / straal * (volgorde.Count - 1);
        if (tot - van < nodig)
        {
            (van, tot) = Verbreed(van, tot, nodig, 0, Math.PI);
        }

        var stap = (tot - van) / (volgorde.Count - 1);
        for (var k = 0; k < volgorde.Count; k++)
        {
            var hoek = van + k * stap;
            plekken[volgorde[k]] = Binnen(
                Opstelling.DirigentX + straal * Math.Cos(hoek),
                Opstelling.DirigentY - straal * Math.Sin(hoek));
        }
    }

    private static void LijnRecht(IReadOnlyList<(double X, double Y)> stoelen, (double X, double Y)[] plekken, List<int> rij)
    {
        var volgorde = rij.OrderBy(i => stoelen[i].X).ToList();
        var y = Mediaan(rij.Select(i => stoelen[i].Y));
        var van = stoelen[volgorde[0]].X;
        var tot = stoelen[volgorde[^1]].X;

        var nodig = MinAfstand * (volgorde.Count - 1);
        if (tot - van < nodig)
        {
            (van, tot) = Verbreed(van, tot, nodig, Diameter / 2, Opstelling.PodiumBreedte - Diameter / 2);
        }

        var stap = (tot - van) / (volgorde.Count - 1);
        for (var k = 0; k < volgorde.Count; k++)
        {
            plekken[volgorde[k]] = Binnen(van + k * stap, y);
        }
    }

    /// <summary>Maakt [van, tot] rond het midden <paramref name="nodig"/> breed, binnen [min, max].</summary>
    private static (double Van, double Tot) Verbreed(double van, double tot, double nodig, double min, double max)
    {
        nodig = Math.Min(nodig, max - min);
        var midden = Math.Clamp((van + tot) / 2, min + nodig / 2, max - nodig / 2);
        return (midden - nodig / 2, midden + nodig / 2);
    }

    /// <summary>Losse stoelen die over een andere stoel heen liggen, een stukje wegschuiven.</summary>
    private static void SchuifLosseStoelen((double X, double Y)[] plekken, bool[] inRij)
    {
        for (var ronde = 0; ronde < 50; ronde++)
        {
            var verschoven = false;
            for (var i = 0; i < plekken.Length; i++)
            {
                for (var j = 0; j < plekken.Length; j++)
                {
                    if (i == j || inRij[i])
                    {
                        continue;
                    }

                    var afstand = Afstand(plekken[i], plekken[j]);
                    if (afstand >= Diameter + 2)
                    {
                        continue;
                    }

                    // Weg van de andere stoel; liggen ze precies op elkaar, dan naar rechts.
                    var (dx, dy) = afstand > 0.01
                        ? ((plekken[i].X - plekken[j].X) / afstand, (plekken[i].Y - plekken[j].Y) / afstand)
                        : (1.0, 0.0);
                    // Twee losse stoelen schuiven allebei de helft; anders schuift alleen de losse.
                    var deel = inRij[j] ? 1.0 : 0.5;
                    var duw = (Diameter + 2 - afstand) * deel;
                    plekken[i] = Binnen(plekken[i].X + dx * duw, plekken[i].Y + dy * duw);
                    verschoven = true;
                }
            }
            if (!verschoven)
            {
                return;
            }
        }
    }

    private static (double X, double Y) Binnen(double x, double y)
    {
        const double straal = Diameter / 2;
        return (Math.Clamp(x, straal, Opstelling.PodiumBreedte - straal),
            Math.Clamp(y, straal, Opstelling.PodiumHoogte - straal));
    }

    private static double Straal((double X, double Y) p)
        => Math.Sqrt(Math.Pow(p.X - Opstelling.DirigentX, 2) + Math.Pow(Opstelling.DirigentY - p.Y, 2));

    /// <summary>Hoek vanaf de dirigent in radialen: 0 = rechts, π/2 = recht vooruit, π = links.</summary>
    private static double Hoek((double X, double Y) p)
        => Math.Atan2(Opstelling.DirigentY - p.Y, p.X - Opstelling.DirigentX);

    private static double Afstand((double X, double Y) a, (double X, double Y) b)
        => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

    private static double Mediaan(IEnumerable<double> waarden)
    {
        var gesorteerd = waarden.Order().ToList();
        var midden = gesorteerd.Count / 2;
        return gesorteerd.Count % 2 == 1 ? gesorteerd[midden] : (gesorteerd[midden - 1] + gesorteerd[midden]) / 2;
    }
}
