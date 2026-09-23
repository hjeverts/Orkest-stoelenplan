# Orkest stoelenplan

Desktop-app om een stoelenplan voor het orkest te maken: musici invoeren, ze naar een
stoel slepen en de opstelling opslaan per concert.

- Start met een klassieke symfonie-opstelling: strijkers in halve cirkels rond de
  dirigent, houtblazers in het midden, koper en slagwerk achteraan. Elke sectie heeft
  een eigen kleur.
- **Musici** voeg je toe met naam en sectie. Sleep een naam uit de lijst naar een stoel
  om iemand te plaatsen. Wie al zit, wordt in de lijst lichter weergegeven.
- **Stoelen** kun je met de muis verslepen, toevoegen (per sectie) en via de
  rechtermuisknop leegmaken of verwijderen.
- **Automatisch indelen** zet iedereen die nog niet zit op een vrije stoel van de eigen
  sectie, van voor (dichtbij de dirigent) naar achter.
- Opstellingen worden op naam opgeslagen, bijvoorbeeld "Najaarsconcert 2026".
- Gebouwd met [Avalonia UI](https://avaloniaui.net/) (.NET), draait op Linux en Windows
  (en macOS).

## Opbouw

```
Orkest.Stoelenplan.slnx
└─ src/
   ├─ Orkest.Stoelenplan.Shared   → modellen (Musicus, Stoel, Opstelling), standaardopstelling,
   │                                   opslag-interface en JSON-bestandsopslag
   ├─ Orkest.Stoelenplan.Desktop  → Avalonia-app (MVVM met CommunityToolkit.Mvvm)
   └─ Orkest.Stoelenplan.Api      → optionele ASP.NET Core API om opstellingen centraal te bewaren
```

De desktop-app praat alleen met `IOpstellingOpslag`. Standaard is dat
`JsonBestandOpslag` (lokale bestanden), en met een backend is het `ApiOpslag` (HTTP).
Beide gebruiken dezelfde modellen en hetzelfde JSON-formaat uit het Shared-project.

## Vereisten

- [.NET SDK 10](https://dotnet.microsoft.com/download) (of nieuwer) om te bouwen.
- Op Linux: een grafische sessie (X11 of Wayland via XWayland) om de app te draaien.

## Bouwen en draaien

```bash
dotnet build
dotnet run --project src/Orkest.Stoelenplan.Desktop
```

Opstellingen worden lokaal bewaard in
`~/.local/share/Orkest.Stoelenplan/opstellingen/` (op Windows in
`%LOCALAPPDATA%\Orkest.Stoelenplan\opstellingen\`), één JSON-bestand per opstelling.

## Met de API

Start de API (luistert standaard op `http://localhost:5731`, instelbaar via `Urls` in
`appsettings.json`):

```bash
dotnet run --project src/Orkest.Stoelenplan.Api
```

En start de desktop-app met de omgevingsvariabele `ORKEST_STOELENPLAN_API_URL`:

```bash
ORKEST_STOELENPLAN_API_URL=http://localhost:5731 dotnet run --project src/Orkest.Stoelenplan.Desktop
```

De API bewaart opstellingen in `src/Orkest.Stoelenplan.Api/data/opstellingen/`
(instelbaar via `Opslag:Map`). Endpoints:

| Methode | Pad                    | Wat                                 |
|---------|------------------------|-------------------------------------|
| GET     | `/opstellingen`        | Namen van alle opgeslagen opstellingen |
| GET     | `/opstellingen/{naam}` | Eén opstelling (404 als die niet bestaat) |
| PUT     | `/opstellingen/{naam}` | Opstelling opslaan of overschrijven |

> **Let op:** de API heeft nog geen authenticatie. Draai hem alleen lokaal of op een
> vertrouwd netwerk zolang dat niet is toegevoegd.

## Privacy

Opstellingen bevatten namen van leden. De map `data/` staat daarom in `.gitignore`,
zodat opgeslagen opstellingen van de API niet per ongeluk in git terechtkomen.

## Licentie

Dit project is MIT-gelicentieerd, zie [LICENSE](LICENSE).
