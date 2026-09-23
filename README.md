# Orkest stoelenplan

Desktop-app om een stoelenplan voor het orkest te maken: musici invoeren, ze naar een
stoel slepen en de opstelling opslaan per concert.

- **Standaardopstellingen** voor vijf soorten orkest, als startpunt om verder aan te
  passen:

  | Orkest          | Opstelling                                                                 |
  |-----------------|----------------------------------------------------------------------------|
  | Symfonieorkest  | Strijkers in halve cirkels rond de dirigent, hout in het midden, koper en slagwerk achterin |
  | Harmonieorkest  | Klarinetten links in drie rijen, hobo's in het midden en fluiten rechts in twee rijen (piccolo en 1e fluit vooraan, 2e fluit erachter); op de 3e rij basklarinetten en fagotten in het midden en de saxofoons rechts (alt vooraan); euphoniums en baritons rechts daarachter, hoorns links vóór de trompetten; achterste blazersrij: trompetten vanuit het midden naar links, trombones, tuba's en baritonsax vanuit het midden naar rechts; slagwerk daarachter |
  | Fanfareorkest   | Bugels vooraan links, saxofoons vooraan rechts, baritons/euphoniums, trompetten en trombones daarachter, bassen en slagwerk achterin |
  | Brassband       | Britse "hoefijzer"-opstelling: solocornetten links vooraan, bugel en althoorns in het midden, euphoniums rechts vooraan; daarachter repiano/2e/3e cornetten, baritons en trombones |
  | Bigband         | Drie rijen (saxofoons, trombones, trompetten) met de leadstemmen in één lijn, ritmesectie links |

- **Instrumenten**: ruim 40 standaardinstrumenten uit al deze orkestsoorten (strijkers,
  houtblazers, saxofoons, koper, slagwerk, ritmesectie, harp en zang), elk met een eigen
  kleur. Via **Instrumenten beheren…** voeg je eigen instrumenten toe (naam, afkorting,
  groep en kleur) of verwijder je ze weer.
- **Musici** voeg je toe met naam en instrument. Sleep een naam uit de lijst naar een
  stoel om iemand te plaatsen. Wie al zit, wordt in de lijst lichter weergegeven.
- **Stoelen** kun je met de muis verslepen, toevoegen (per instrument, met optioneel een
  partij zoals "1", "2" of "Solo") en via de rechtermuisknop leegmaken of verwijderen.
- **Automatisch indelen** zet iedereen die nog niet zit op een vrije stoel van het eigen
  instrument: eerst de hoogste partij (Solo, 1, Rep, 2, …), en daarbinnen van voor
  (dichtbij de dirigent) naar achter.
- Opstellingen worden op naam opgeslagen, bijvoorbeeld "Najaarsconcert 2026". Eigen
  instrumenten die in een opstelling gebruikt worden, worden erin meegeslagen, zodat
  de opstelling ook op een andere computer te openen is.
- Gebouwd met [Avalonia UI](https://avaloniaui.net/) (.NET), draait op Linux en Windows
  (en macOS).

## Opbouw

```
Orkest.Stoelenplan.slnx
└─ src/
   ├─ Orkest.Stoelenplan.Shared   → modellen (Instrument, Musicus, Stoel, Opstelling),
   │                                standaardinstrumenten en -opstellingen,
   │                                opslag-interface en JSON-bestandsopslag
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

Gegevens worden lokaal bewaard in `~/.local/share/Orkest.Stoelenplan/` (op Windows in
`%LOCALAPPDATA%\Orkest.Stoelenplan\`):

- `opstellingen/` – één JSON-bestand per opstelling;
- `instrumenten.json` – de eigen instrumenten (de standaardinstrumenten zitten in de app zelf).

## Uitleveren per platform

De app wordt uitgeleverd als **één zelfstandig uitvoerbaar bestand** (self-contained,
single-file): op de doelmachine is geen .NET nodig. Je kunt voor elk platform vanaf
elk platform bouwen, dus bijvoorbeeld op Linux ook de Windows- en macOS-versie maken.

Het commando is steeds hetzelfde, alleen de *runtime identifier* (`-r`) verschilt:

```bash
dotnet publish src/Orkest.Stoelenplan.Desktop -c Release -r <rid> \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -p:DebugType=none \
  -o publish/<rid>
```

| Platform                                                  | `<rid>`       | Resultaat                                 |
|-----------------------------------------------------------|---------------|-------------------------------------------|
| Windows x64                                               | `win-x64`     | `publish/win-x64/OrkestStoelenplan.exe`   |
| Linux x64 (CachyOS, Arch, Debian, Ubuntu, openSUSE, Fedora) | `linux-x64` | `publish/linux-x64/OrkestStoelenplan`     |
| macOS met Apple Silicon (M1 en nieuwer)                   | `osx-arm64`   | `publish/osx-arm64/OrkestStoelenplan`     |
| macOS met Intel-processor                                 | `osx-x64`     | `publish/osx-x64/OrkestStoelenplan`       |

Het resultaat is ongeveer 50 MB. De map `publish/` staat in `.gitignore`.

Alles in één keer bouwen (bash):

```bash
for rid in win-x64 linux-x64 osx-arm64 osx-x64; do
  dotnet publish src/Orkest.Stoelenplan.Desktop -c Release -r "$rid" \
    --self-contained true -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true \
    -p:DebugType=none -o "publish/$rid"
done
```

Bouw je op Windows in PowerShell, zet het commando dan op één regel (of vervang de
`\` aan het einde van de regels door een backtick `` ` ``).

### .NET SDK installeren (alleen nodig om te bouwen)

| Systeem           | Installeren                                                                |
|-------------------|----------------------------------------------------------------------------|
| CachyOS / Arch    | `sudo pacman -S dotnet-sdk`                                                |
| Fedora            | `sudo dnf install dotnet-sdk-10.0`                                         |
| Ubuntu            | `sudo apt install dotnet-sdk-10.0`                                         |
| Debian / openSUSE | Via het install-script hieronder (of de pakketbron van Microsoft)          |
| macOS             | Installer van [dotnet.microsoft.com](https://dotnet.microsoft.com/download) of `brew install --cask dotnet-sdk` |
| Windows           | Installer van [dotnet.microsoft.com](https://dotnet.microsoft.com/download) of `winget install Microsoft.DotNet.SDK.10` |

Werkt dat niet (bijvoorbeeld omdat je distributie nog geen .NET 10 heeft), dan werkt
het install-script van Microsoft op elke Linux-distributie en op macOS, zonder root:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0
export PATH="$HOME/.dotnet:$PATH"   # ook in ~/.bashrc of ~/.zshrc zetten
```

### Windows

Kopieer `OrkestStoelenplan.exe` naar de doelcomputer en start hem met een dubbelklik.
Omdat het bestand niet digitaal ondertekend is, kan Windows SmartScreen de eerste keer
waarschuwen: kies **Meer informatie → Toch uitvoeren**.

### Linux

Hetzelfde `linux-x64`-bestand werkt op alle genoemde distributies. Na kopiëren
(bijv. via een USB-stick of download) moet het soms opnieuw uitvoerbaar gemaakt worden:

```bash
chmod +x OrkestStoelenplan
./OrkestStoelenplan
```

De app heeft een paar gangbare systeembibliotheken nodig (ICU, fontconfig, X11). Op een
installatie met een desktopomgeving zijn die er vrijwel altijd al; ontbreekt er toch
iets, dan:

| Distributie      | Commando                                                              |
|------------------|-----------------------------------------------------------------------|
| CachyOS / Arch   | `sudo pacman -S --needed icu fontconfig libx11 libice libsm`          |
| Debian / Ubuntu  | `sudo apt install libicu-dev libfontconfig1 libx11-6 libice6 libsm6`  |
| Fedora           | `sudo dnf install libicu fontconfig libX11 libICE libSM`              |
| openSUSE         | `sudo zypper install libicu-devel fontconfig libX11-6 libICE6 libSM6` |

Onder Wayland (standaard op o.a. Fedora, Ubuntu en openSUSE met GNOME of KDE) draait de
app via XWayland; dat is op die systemen standaard aanwezig.

Om de app in het startmenu te krijgen, zet je het bestand bijvoorbeeld in
`~/.local/bin/` en maak je `~/.local/share/applications/orkest-stoelenplan.desktop` aan:

```ini
[Desktop Entry]
Type=Application
Name=Orkest stoelenplan
Exec=/home/<gebruiker>/.local/bin/OrkestStoelenplan
Terminal=false
Categories=Office;
```

### macOS

Kies `osx-arm64` voor Macs met een M-chip (Apple-menu → **Over deze Mac** toont
"Chip Apple M…") en `osx-x64` voor oudere Macs met een Intel-processor.

Het resultaat is een los uitvoerbaar bestand, nog geen `.app`-bundel. Omdat het niet
door Apple ondertekend is, blokkeert Gatekeeper het na downloaden. Zo start je het:

```bash
chmod +x OrkestStoelenplan
xattr -d com.apple.quarantine OrkestStoelenplan   # Gatekeeper-blokkade opheffen
./OrkestStoelenplan
```

Verpak het bestand voor het overzetten liefst als `.tar.gz` of `.zip` die op de Mac
zelf wordt uitgepakt; dan blijft het uitvoerbaar.

### Snelste lokale build (CachyOS met Hyprland)

Voor eigen gebruik op de machine waar je ook bouwt, kun je de opstarttijd verder
verkorten:

- **ReadyToRun** (`PublishReadyToRun=true`) compileert de code vooraf naar machinecode,
  zodat er bij het opstarten nauwelijks JIT-werk is. Code die vaak draait wordt
  tijdens gebruik nog steeds geoptimaliseerd voor jouw CPU (AVX2/AVX-512 waar
  beschikbaar), vergelijkbaar met de x86-64-v3/v4-pakketten van CachyOS.
- **Geen compressie**: het bestand wordt groter (ca. 125 MB), maar hoeft bij het
  starten niet eerst uitgepakt te worden.

```bash
dotnet publish src/Orkest.Stoelenplan.Desktop -c Release -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:PublishReadyToRun=true \
  -p:DebugType=none \
  -o publish/linux-x64-lokaal

install -Dm755 publish/linux-x64-lokaal/OrkestStoelenplan ~/.local/bin/OrkestStoelenplan
```

Draai je CachyOS op ARM, gebruik dan `-r linux-arm64`.

**Hyprland** is een Wayland-compositor; Avalonia draait daarbinnen via **XWayland**.

1. Zorg dat XWayland geïnstalleerd is (meestal al het geval):

   ```bash
   pacman -Q xorg-xwayland || sudo pacman -S xorg-xwayland
   ```

2. Hyprland zet het venster standaard in een tegel, terwijl de app minimaal 900×600
   nodig heeft. Laat hem liever zwevend openen via `~/.config/hypr/hyprland.conf`:

   ```ini
   windowrulev2 = float, title:^(Orkest stoelenplan)$
   windowrulev2 = size 1400 850, title:^(Orkest stoelenplan)$
   ```

   (Nieuwere Hyprland-versies gebruiken een aangepaste `windowrule`-syntax; zie de
   [Hyprland-wiki](https://wiki.hypr.land/Configuring/Window-Rules/) als deze regels
   een foutmelding geven.)

3. Gebruik je fractionele schaling (bijv. `1.5` in je `monitor=`-regel), dan kunnen
   XWayland-apps wazig ogen. Laat XWayland dan onschaald renderen en laat de app zelf
   schalen:

   ```ini
   xwayland {
       force_zero_scaling = true
   }
   ```

   En start de app met de bijbehorende schaalfactor, bijvoorbeeld via
   `~/.local/share/applications/orkest-stoelenplan.desktop` (verschijnt dan ook in
   launchers als wofi, rofi of fuzzel):

   ```ini
   [Desktop Entry]
   Type=Application
   Name=Orkest stoelenplan
   Exec=env AVALONIA_GLOBAL_SCALE_FACTOR=1.5 /home/<gebruiker>/.local/bin/OrkestStoelenplan
   Terminal=false
   Categories=Office;
   ```

   Zonder fractionele schaling is `force_zero_scaling` en de `env`-variabele niet nodig.

Onder **XFCE** (X11) zijn geen extra stappen nodig.

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

De API bewaart opstellingen en eigen instrumenten in `src/Orkest.Stoelenplan.Api/data/`
(instelbaar via `Opslag:Map`), in dezelfde indeling als lokaal. Endpoints:

| Methode | Pad                    | Wat                                 |
|---------|------------------------|-------------------------------------|
| GET     | `/opstellingen`        | Namen van alle opgeslagen opstellingen |
| GET     | `/opstellingen/{naam}` | Eén opstelling (404 als die niet bestaat) |
| PUT     | `/opstellingen/{naam}` | Opstelling opslaan of overschrijven |
| GET     | `/instrumenten`        | Eigen instrumenten                  |
| PUT     | `/instrumenten`        | Eigen instrumenten vervangen        |

> **Let op:** de API heeft nog geen authenticatie. Draai hem alleen lokaal of op een
> vertrouwd netwerk zolang dat niet is toegevoegd.

## Privacy

Opstellingen bevatten namen van leden. De map `data/` staat daarom in `.gitignore`,
zodat opgeslagen opstellingen van de API niet per ongeluk in git terechtkomen.

## Licentie

Dit project is MIT-gelicentieerd, zie [LICENSE](LICENSE).
