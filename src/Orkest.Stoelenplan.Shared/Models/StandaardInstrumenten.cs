namespace Orkest.Stoelenplan.Shared.Models;

/// <summary>
/// Alle instrumenten die in een symfonieorkest, harmonie, fanfare, brassband of bigband
/// voorkomen. De <c>Id</c>'s worden opgeslagen in opstellingen, dus niet zomaar wijzigen.
/// </summary>
public static class StandaardInstrumenten
{
    // Strijkers
    public const string Viool = "viool";
    public const string Altviool = "altviool";
    public const string Cello = "cello";
    public const string Contrabas = "contrabas";

    // Houtblazers
    public const string Piccolo = "piccolo";
    public const string Fluit = "fluit";
    public const string Hobo = "hobo";
    public const string EngelseHoorn = "engelse-hoorn";
    public const string EsKlarinet = "es-klarinet";
    public const string Klarinet = "klarinet";
    public const string Altklarinet = "altklarinet";
    public const string Basklarinet = "basklarinet";
    public const string Fagot = "fagot";
    public const string Contrafagot = "contrafagot";

    // Saxofoons
    public const string Sopraansaxofoon = "sopraansaxofoon";
    public const string Altsaxofoon = "altsaxofoon";
    public const string Tenorsaxofoon = "tenorsaxofoon";
    public const string Baritonsaxofoon = "baritonsaxofoon";

    // Koper
    public const string Sopraancornet = "sopraancornet";
    public const string Cornet = "cornet";
    public const string Trompet = "trompet";
    public const string Bugel = "bugel";
    public const string Althoorn = "althoorn";
    public const string Hoorn = "hoorn";
    public const string Bariton = "bariton";
    public const string Euphonium = "euphonium";
    public const string Trombone = "trombone";
    public const string Bastrombone = "bastrombone";
    public const string EsBas = "es-bas";
    public const string BesBas = "bes-bas";
    public const string Tuba = "tuba";

    // Slagwerk
    public const string Pauken = "pauken";
    public const string Slagwerk = "slagwerk";
    public const string Mallets = "mallets";

    // Ritmesectie
    public const string Drumstel = "drumstel";
    public const string Piano = "piano";
    public const string Gitaar = "gitaar";
    public const string Basgitaar = "basgitaar";

    // Overig
    public const string Harp = "harp";
    public const string Zang = "zang";

    public static IReadOnlyList<Instrument> Alle { get; } =
    [
        Maak(Viool, "Viool", "Vl", InstrumentGroep.Strijkers, "#F4A6A6"),
        Maak(Altviool, "Altviool", "Alt", InstrumentGroep.Strijkers, "#F3D9A4"),
        Maak(Cello, "Cello", "Vc", InstrumentGroep.Strijkers, "#D9B8E8"),
        Maak(Contrabas, "Contrabas", "Cb", InstrumentGroep.Strijkers, "#C3B1E1"),

        Maak(Piccolo, "Piccolo", "Picc", InstrumentGroep.Houtblazers, "#D4EDD4"),
        Maak(Fluit, "Dwarsfluit", "Fl", InstrumentGroep.Houtblazers, "#BFE3C0"),
        Maak(Hobo, "Hobo", "Hb", InstrumentGroep.Houtblazers, "#A8D8B0"),
        Maak(EngelseHoorn, "Engelse hoorn", "EH", InstrumentGroep.Houtblazers, "#93CFA0"),
        Maak(EsKlarinet, "Es-klarinet", "Es-kl", InstrumentGroep.Houtblazers, "#B8E6DC"),
        Maak(Klarinet, "Klarinet", "Kl", InstrumentGroep.Houtblazers, "#9FD3C7"),
        Maak(Altklarinet, "Altklarinet", "Akl", InstrumentGroep.Houtblazers, "#8CCBBE"),
        Maak(Basklarinet, "Basklarinet", "Bkl", InstrumentGroep.Houtblazers, "#7FC3B4"),
        Maak(Fagot, "Fagot", "Fg", InstrumentGroep.Houtblazers, "#B5D99C"),
        Maak(Contrafagot, "Contrafagot", "Cfg", InstrumentGroep.Houtblazers, "#A3CF86"),

        Maak(Sopraansaxofoon, "Sopraansaxofoon", "S.sax", InstrumentGroep.Saxofoons, "#CFE0F7"),
        Maak(Altsaxofoon, "Altsaxofoon", "A.sax", InstrumentGroep.Saxofoons, "#B7D0F2"),
        Maak(Tenorsaxofoon, "Tenorsaxofoon", "T.sax", InstrumentGroep.Saxofoons, "#A0C0EC"),
        Maak(Baritonsaxofoon, "Baritonsaxofoon", "B.sax", InstrumentGroep.Saxofoons, "#8DB2E6"),

        Maak(Sopraancornet, "Sopraancornet (Es)", "Sop", InstrumentGroep.Koper, "#FFF2B3"),
        Maak(Cornet, "Cornet", "Cor", InstrumentGroep.Koper, "#FFE38A"),
        Maak(Trompet, "Trompet", "Tp", InstrumentGroep.Koper, "#FFD166"),
        Maak(Bugel, "Bugel", "Bug", InstrumentGroep.Koper, "#FFDC7A"),
        Maak(Althoorn, "Althoorn (Es)", "Ahn", InstrumentGroep.Koper, "#F9D8A8"),
        Maak(Hoorn, "Hoorn", "Hn", InstrumentGroep.Koper, "#F7C98B"),
        Maak(Bariton, "Bariton", "Bar", InstrumentGroep.Koper, "#F4B97A"),
        Maak(Euphonium, "Euphonium", "Euph", InstrumentGroep.Koper, "#F0A868"),
        Maak(Trombone, "Trombone", "Tb", InstrumentGroep.Koper, "#FFC078"),
        Maak(Bastrombone, "Bastrombone", "Btb", InstrumentGroep.Koper, "#F5AE63"),
        Maak(EsBas, "Es-bas", "Es-b", InstrumentGroep.Koper, "#E8B48A"),
        Maak(BesBas, "Bes-bas", "Bes-b", InstrumentGroep.Koper, "#DDA276"),
        Maak(Tuba, "Tuba", "Tu", InstrumentGroep.Koper, "#F5B971"),

        Maak(Pauken, "Pauken", "Pk", InstrumentGroep.Slagwerk, "#D5D9E0"),
        Maak(Slagwerk, "Slagwerk", "Slw", InstrumentGroep.Slagwerk, "#C9CED6"),
        Maak(Mallets, "Mallets (xylofoon, marimba, …)", "Mal", InstrumentGroep.Slagwerk, "#B9C0CB"),

        Maak(Drumstel, "Drumstel", "Dr", InstrumentGroep.Ritmesectie, "#AEB6C3"),
        Maak(Piano, "Piano / toetsen", "Pno", InstrumentGroep.Ritmesectie, "#B8C4E0"),
        Maak(Gitaar, "Gitaar", "Git", InstrumentGroep.Ritmesectie, "#E6C8B0"),
        Maak(Basgitaar, "Basgitaar", "Bg", InstrumentGroep.Ritmesectie, "#D8B49A"),

        Maak(Harp, "Harp", "Hp", InstrumentGroep.Overig, "#A9CCE8"),
        Maak(Zang, "Zang", "Zang", InstrumentGroep.Overig, "#F2C6DE"),
    ];

    private static readonly HashSet<string> Ids = Alle.Select(i => i.Id).ToHashSet();

    public static bool IsStandaard(string id) => Ids.Contains(id);

    private static Instrument Maak(string id, string naam, string afkorting, InstrumentGroep groep, string kleur)
        => new() { Id = id, Naam = naam, Afkorting = afkorting, Groep = groep, Kleur = kleur };
}
