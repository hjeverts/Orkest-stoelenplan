#!/usr/bin/env bash
# Maakt een nieuwe release van Orkest stoelenplan en publiceert die op GitHub.
#
#   scripts/release.sh <versie> [release-notes.md] [--dry-run] [-y]
#
#   <versie>            bv. 1.2.0 (zonder "v"); de tag wordt v<versie>
#   release-notes.md    optioneel: tekst voor de release; zonder bestand maakt GitHub
#                       automatisch een lijst van de wijzigingen sinds de vorige release
#   --dry-run           alleen bouwen en inpakken (in publish/release/), niets publiceren
#   -y                  niet om bevestiging vragen voor het publiceren
#
# Stappen: controleren (schone werkmap, op main, tag bestaat nog niet, gh ingelogd),
# versie in het project zetten en committen, bouwen voor Windows, Linux en macOS
# (Apple Silicon en Intel), inpakken, main pushen en de release met bestanden aanmaken.

set -euo pipefail

cd "$(dirname "$0")/.."

PROJECT=src/Orkest.Stoelenplan.Desktop
CSPROJ=$PROJECT/Orkest.Stoelenplan.Desktop.csproj
NAAM=OrkestStoelenplan
PLATFORMS=(win-x64 linux-x64 osx-arm64 osx-x64)

fout() { echo "Fout: $*" >&2; exit 1; }
stap() { echo; echo "==> $*"; }

versie=""
notes=""
dry_run=false
ja=false
for arg in "$@"; do
    case "$arg" in
        --dry-run) dry_run=true ;;
        -y) ja=true ;;
        -h|--help) sed -n '2,15p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
        -*) fout "onbekende optie $arg" ;;
        *)
            if [[ -z "$versie" ]]; then versie=$arg
            elif [[ -z "$notes" ]]; then notes=$arg
            else fout "te veel argumenten"
            fi ;;
    esac
done

[[ -n "$versie" ]] || fout "geef een versie op, bv. scripts/release.sh 1.2.0"
versie=${versie#v}
[[ "$versie" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || fout "versie moet de vorm 1.2.3 hebben, niet '$versie'"
tag=v$versie
[[ -z "$notes" || -f "$notes" ]] || fout "release-notes '$notes' niet gevonden"

# --- Controles --------------------------------------------------------------------

stap "Controleren"
command -v dotnet >/dev/null || fout "dotnet (SDK 10) is niet geïnstalleerd"
[[ -z "$(git status --porcelain)" ]] || fout "de werkmap heeft niet-gecommitte wijzigingen; commit of stash die eerst"
if ! $dry_run; then
    command -v gh >/dev/null || fout "GitHub CLI (gh) is niet geïnstalleerd"
    gh auth status >/dev/null 2>&1 || fout "niet ingelogd bij GitHub; voer 'gh auth login' uit"
    [[ "$(git branch --show-current)" == main ]] || fout "releases worden vanaf main gemaakt"
    git fetch --quiet origin
    [[ -z "$(git rev-list HEAD..origin/main)" ]] || fout "origin/main heeft commits die hier nog niet zijn; eerst 'git pull'"
    ! git rev-parse -q --verify "refs/tags/$tag" >/dev/null || fout "tag $tag bestaat al"
    ! gh release view "$tag" >/dev/null 2>&1 || fout "release $tag bestaat al op GitHub"
fi
echo "Versie $versie, tag $tag$($dry_run && echo ' (dry-run: er wordt niets gepubliceerd)')"

# --- Versie zetten ----------------------------------------------------------------

huidig=$(sed -n 's|.*<Version>\(.*\)</Version>.*|\1|p' "$CSPROJ")
if [[ "$huidig" != "$versie" ]]; then
    if $dry_run; then
        echo "Projectversie is $huidig; bij een echte release wordt die $versie (dry-run: niet aangepast)"
    else
        stap "Versie $huidig → $versie"
        sed -i.bak "s|<Version>$huidig</Version>|<Version>$versie</Version>|" "$CSPROJ" && rm -f "$CSPROJ.bak"
        git commit --quiet -m "Set version to $versie" -- "$CSPROJ"
        git log --oneline -1
    fi
else
    echo "Projectversie staat al op $versie"
fi

# --- Bouwen -----------------------------------------------------------------------

rm -rf publish
for rid in "${PLATFORMS[@]}"; do
    stap "Bouwen voor $rid"
    dotnet publish "$PROJECT" -c Release -r "$rid" \
        --self-contained true -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true \
        -p:DebugType=none -o "publish/$rid" -v quiet -nologo
done

# --- Inpakken ---------------------------------------------------------------------
# Windows en Linux als los bestand; macOS als .tar.gz, zodat het bestand na downloaden
# uitvoerbaar blijft.

stap "Inpakken"
uit=publish/release
mkdir -p "$uit"
bestanden=()
for rid in "${PLATFORMS[@]}"; do
    case "$rid" in
        win-*)
            doel=$uit/$NAAM-$versie-$rid.exe
            cp "publish/$rid/$NAAM.exe" "$doel" ;;
        linux-*)
            doel=$uit/$NAAM-$versie-$rid
            cp "publish/$rid/$NAAM" "$doel"
            chmod +x "$doel" ;;
        osx-*)
            doel=$uit/$NAAM-$versie-$rid.tar.gz
            chmod +x "publish/$rid/$NAAM"
            tar -czf "$doel" -C "publish/$rid" "$NAAM" ;;
    esac
    bestanden+=("$doel")
done
ls -lh "${bestanden[@]}"

if $dry_run; then
    stap "Dry-run klaar; de bestanden staan in $uit/"
    exit 0
fi

# --- Publiceren -------------------------------------------------------------------

if ! $ja; then
    echo
    read -r -p "main pushen en release $tag op GitHub publiceren? [j/N] " antwoord
    [[ "$antwoord" =~ ^[jJyY]$ ]] || { echo "Gestopt; er is niets gepubliceerd (de versie-commit staat wel lokaal)."; exit 1; }
fi

stap "main pushen"
git push --quiet origin main

stap "Release $tag aanmaken"
notes_optie=(--generate-notes)
[[ -n "$notes" ]] && notes_optie=(--notes-file "$notes")
gh release create "$tag" "${bestanden[@]}" \
    --target main \
    --title "Orkest stoelenplan $versie" \
    "${notes_optie[@]}"

git fetch --quiet --tags origin
stap "Klaar: $(gh release view "$tag" --json url -q .url)"
