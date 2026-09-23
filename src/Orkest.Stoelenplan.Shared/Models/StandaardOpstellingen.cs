using static Orkest.Stoelenplan.Shared.Models.StandaardInstrumenten;

namespace Orkest.Stoelenplan.Shared.Models;

public enum OrkestType
{
    Symfonieorkest,
    Harmonieorkest,
    Fanfareorkest,
    Brassband,
    Bigband,
}

/// <summary>
/// Gangbare opstellingen per orkesttype, gezien vanaf de dirigent (publiek onderaan).
/// Het zijn startpunten: stoelen kunnen daarna vrij verschoven, toegevoegd en verwijderd worden.
/// </summary>
public static class StandaardOpstellingen
{
    private readonly record struct Groep(string InstrumentId, int Aantal, string Partij);

    private static Groep G(string instrumentId, int aantal, string partij = "") => new(instrumentId, aantal, partij);

    public static List<Stoel> Maak(OrkestType type) => type switch
    {
        OrkestType.Symfonieorkest => Symfonieorkest(),
        OrkestType.Harmonieorkest => Harmonieorkest(),
        OrkestType.Fanfareorkest => Fanfareorkest(),
        OrkestType.Brassband => Brassband(),
        OrkestType.Bigband => Bigband(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    /// <summary>Strijkers in halve cirkels, hout in het midden, koper en slagwerk achterin.</summary>
    private static List<Stoel> Symfonieorkest()
    {
        var s = new List<Stoel>();
        Boog(s, 170, 180, 0, G(Viool, 2, "1"), G(Viool, 2, "2"), G(Altviool, 2), G(Cello, 2));
        Boog(s, 250, 180, 0, G(Viool, 3, "1"), G(Viool, 3, "2"), G(Altviool, 3), G(Cello, 3));
        Boog(s, 330, 180, 0, G(Viool, 4, "1"), G(Viool, 4, "2"), G(Altviool, 4), G(Cello, 4));
        Boog(s, 410, 175, 150, G(Harp, 1), G(Piano, 1));
        Boog(s, 410, 125, 55, G(Piccolo, 1), G(Fluit, 2), G(Hobo, 2), G(EngelseHoorn, 1));
        Boog(s, 410, 40, 10, G(Contrabas, 3));
        Boog(s, 490, 175, 130, G(Hoorn, 4));
        Boog(s, 490, 120, 60, G(Klarinet, 2), G(Basklarinet, 1), G(Fagot, 2), G(Contrafagot, 1));
        Boog(s, 570, 150, 125, G(Trompet, 3));
        Boog(s, 570, 120, 85, G(Trombone, 2), G(Bastrombone, 1), G(Tuba, 1));
        Boog(s, 570, 80, 70, G(Pauken, 1));
        Boog(s, 570, 65, 35, G(Slagwerk, 3));
        return s;
    }

    /// <summary>
    /// Klarinetten links in drie rijen, hobo's in het midden en fluiten rechts in twee rijen;
    /// basklarinetten en fagotten op de 3e rij in het midden en de saxofoons rechts daarnaast,
    /// achter de fluiten. Euphoniums en baritons rechts daarachter, hoorns links vóór de
    /// trompetten. Op de achterste blazersrij beginnen trompetten (naar links) en trombones
    /// (naar rechts) in het midden, met de tuba's en de baritonsax rechts ernaast; het
    /// slagwerk staat daarachter.
    /// </summary>
    private static List<Stoel> Harmonieorkest()
    {
        var s = new List<Stoel>();
        // Rij 1: 1e klarinetten – hobo's – 1e fluiten en piccolo
        Boog(s, 190, 180, 0, G(Klarinet, 3, "1"), G(EsKlarinet, 1), G(Hobo, 2), G(Fluit, 2, "1"), G(Piccolo, 1));
        // Rij 2: 2e klarinetten links, 2e fluiten rechts
        Boog(s, 265, 180, 110, G(Klarinet, 4, "2"));
        Boog(s, 265, 70, 0, G(Fluit, 4, "2"));
        // Rij 3: 3e klarinetten – basklarinetten en fagotten – saxofoons (alt vooraan)
        Boog(s, 340, 180, 105, G(Klarinet, 5, "3"), G(Altklarinet, 1));
        Boog(s, 340, 105, 60, G(Basklarinet, 2), G(Fagot, 2));
        Boog(s, 340, 60, 0, G(Altsaxofoon, 1, "1"), G(Altsaxofoon, 1, "2"), G(Tenorsaxofoon, 2));
        // Rij 4: hoorns links vóór de trompetten; euphoniums en baritons rechts
        Boog(s, 415, 150, 112, G(Hoorn, 4));
        Boog(s, 415, 58, 2, G(Euphonium, 2), G(Bariton, 2));
        // Rij 5: trompetten vanuit het midden naar links; trombones, tuba's en baritonsax
        // vanuit het midden naar rechts
        Boog(s, 490, 91, 145, G(Trompet, 2, "1"), G(Trompet, 2, "2"), G(Trompet, 2, "3"));
        Boog(s, 490, 89, 18, G(Trombone, 1, "1"), G(Trombone, 1, "2"), G(Trombone, 1, "3"), G(Bastrombone, 1),
            G(Tuba, 3), G(Baritonsaxofoon, 1));
        // Achteraan: slagwerk
        Boog(s, 565, 110, 70, G(Pauken, 1), G(Slagwerk, 3), G(Mallets, 1));
        return s;
    }

    /// <summary>Bugels vooraan links, saxofoons vooraan rechts, koper en bassen daarachter.</summary>
    private static List<Stoel> Fanfareorkest()
    {
        var s = new List<Stoel>();
        Boog(s, 170, 180, 0, G(Bugel, 2, "1"), G(Bugel, 2, "2"), G(Sopraansaxofoon, 1),
            G(Altsaxofoon, 1, "1"), G(Altsaxofoon, 1, "2"), G(Tenorsaxofoon, 1));
        Boog(s, 250, 180, 0, G(Bugel, 2, "1"), G(Bugel, 2, "2"), G(Althoorn, 2), G(Bariton, 2),
            G(Euphonium, 2), G(Tenorsaxofoon, 1), G(Baritonsaxofoon, 1));
        Boog(s, 330, 170, 10, G(Trompet, 2, "1"), G(Trompet, 2, "2"), G(Hoorn, 3),
            G(Trombone, 1, "1"), G(Trombone, 1, "2"), G(Trombone, 1, "3"), G(Bastrombone, 1));
        Boog(s, 410, 120, 60, G(EsBas, 2), G(BesBas, 2));
        Boog(s, 490, 110, 70, G(Pauken, 1), G(Slagwerk, 3));
        return s;
    }

    /// <summary>
    /// Klassieke Britse "hoefijzer"-opstelling: solocornetten links vooraan, bugel en
    /// althoorns in het midden, euphoniums rechts vooraan; daarachter de overige cornetten,
    /// baritons en trombones, dan de bassen en achteraan het slagwerk.
    /// </summary>
    private static List<Stoel> Brassband()
    {
        var s = new List<Stoel>();
        Boog(s, 220, 180, 0, G(Cornet, 4, "Solo"), G(Bugel, 1), G(Althoorn, 1, "Solo"),
            G(Althoorn, 1, "1"), G(Althoorn, 1, "2"), G(Euphonium, 2));
        Boog(s, 320, 180, 0, G(Sopraancornet, 1), G(Cornet, 1, "Rep"), G(Cornet, 2, "2"), G(Cornet, 2, "3"),
            G(Bariton, 1, "1"), G(Bariton, 1, "2"), G(Trombone, 1, "1"), G(Trombone, 1, "2"), G(Bastrombone, 1));
        Boog(s, 420, 115, 65, G(EsBas, 2), G(BesBas, 2));
        Boog(s, 520, 125, 55, G(Pauken, 1), G(Slagwerk, 3), G(Mallets, 1));
        return s;
    }

    /// <summary>
    /// Drie rijen blazers (saxofoons, trombones, trompetten) met de leadstemmen in één lijn,
    /// en de ritmesectie links.
    /// </summary>
    private static List<Stoel> Bigband()
    {
        var s = new List<Stoel>();
        Rij(s, 520, 500, 80, G(Baritonsaxofoon, 1), G(Tenorsaxofoon, 1, "2"), G(Altsaxofoon, 1, "2"),
            G(Altsaxofoon, 1, "1"), G(Tenorsaxofoon, 1, "1"));
        Rij(s, 410, 580, 80, G(Bastrombone, 1), G(Trombone, 1, "3"), G(Trombone, 1, "1"), G(Trombone, 1, "2"));
        Rij(s, 300, 580, 80, G(Trompet, 1, "4"), G(Trompet, 1, "3"), G(Trompet, 1, "1"), G(Trompet, 1, "2"));
        Los(s, Piano, 200, 510);
        Los(s, Gitaar, 310, 560);
        Los(s, Basgitaar, 270, 400);
        Los(s, Drumstel, 390, 300);
        Los(s, Zang, 1000, 590);
        return s;
    }

    /// <summary>
    /// Zet de groepen na elkaar op een cirkelboog rond de dirigent. Hoeken in graden:
    /// 180 = helemaal links, 90 = recht vooruit (achter op het podium), 0 = helemaal rechts.
    /// </summary>
    private static void Boog(List<Stoel> stoelen, double straal, double vanHoek, double totHoek, params Groep[] groepen)
    {
        var stap = (totHoek - vanHoek) / groepen.Sum(g => g.Aantal);
        var i = 0;
        foreach (var groep in groepen)
        {
            for (var k = 0; k < groep.Aantal; k++, i++)
            {
                var hoek = (vanHoek + (i + 0.5) * stap) * Math.PI / 180;
                Voeg(stoelen, groep, Opstelling.DirigentX + straal * Math.Cos(hoek),
                    Opstelling.DirigentY - straal * Math.Sin(hoek));
            }
        }
    }

    /// <summary>Zet de groepen na elkaar op een rechte rij, van links naar rechts.</summary>
    private static void Rij(List<Stoel> stoelen, double y, double vanX, double stapX, params Groep[] groepen)
    {
        var i = 0;
        foreach (var groep in groepen)
        {
            for (var k = 0; k < groep.Aantal; k++, i++)
            {
                Voeg(stoelen, groep, vanX + i * stapX, y);
            }
        }
    }

    private static void Los(List<Stoel> stoelen, string instrumentId, double x, double y)
        => Voeg(stoelen, G(instrumentId, 1), x, y);

    private static void Voeg(List<Stoel> stoelen, Groep groep, double x, double y)
        => stoelen.Add(new Stoel
        {
            InstrumentId = groep.InstrumentId,
            Partij = groep.Partij,
            X = Math.Round(x),
            Y = Math.Round(y),
        });
}
