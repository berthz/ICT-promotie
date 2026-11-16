# ICT-promotie · CHE Ede

Webapp voor het team van de ICT-opleiding aan de CHE in Ede. Doel: meer bezoekers op de open dagen en open avonden.

- **Kalender (homepage):** interne promotieactiviteiten, externe activiteiten op locatie en open dagen/avonden, in kleur.
- **Activiteiten:** toevoegen, wijzigen en verwijderen, met wie de activiteit geeft en welk materiaal nodig is.
  De app waarschuwt als iemand of iets op dezelfde dag dubbel is ingepland.
- **Mensen** en **Materiaal:** beheren wie activiteiten geeft en wat er aan promotiemateriaal is.
- **Ideeën:** iedereen kan een idee toevoegen om de opleiding met weinig moeite te promoten, en stemmen op ideeën.
- **Nieuws:** lokaal en landelijk nieuws waarin ICT aan de CHE voorkomt. Wordt elke paar uur automatisch via Google Nieuws
  opgehaald; je kunt ook zelf berichten toevoegen, en berichten die niet over ons gaan verbergen.
- **Schermweergave** (`/Scherm`): voor het grote scherm in de ICT-ruimte. Geen menu, grote kalender, aftellen tot de
  volgende open dag, en ververst zichzelf elke 5 minuten.

Er is (nog) geen inlog: iedereen die de pagina kan bereiken, kan alles wijzigen.

## Techniek

ASP.NET Core 8 (Razor Pages, C#), Entity Framework Core en SQLite. Eén programma, geen aparte databaseserver nodig.

| Map | Inhoud |
| --- | --- |
| `src/IctPromotie/Data` | Datamodel, databasecontext, voorbeelddata |
| `src/IctPromotie/Pages` | Alle pagina's (per onderdeel een map met `Index` en `Bewerken`) |
| `src/IctPromotie/Services` | Automatisch nieuws ophalen |
| `.devcontainer` | Configuratie voor GitHub Codespaces |
| `.github/workflows` | GitHub Actions: bouwt de app en maakt een Windows-versie |

## Ontwikkelen

**Op je eigen pc:** installeer de [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) en start:

```
dotnet run --project src/IctPromotie
```

Open daarna <http://localhost:5000>.

**In de browser (GitHub Codespaces):** op GitHub: *Code → Codespaces → Create codespace on main*. Typ in de terminal
hetzelfde `dotnet run`-commando; Codespaces opent de app vanzelf.

## Op de pc in de ICT-ruimte zetten

1. Ga op GitHub naar **Actions → Bouwen**, open de laatste geslaagde run en download **IctPromotie-windows**.
2. Pak het zip-bestand uit in een map **buiten OneDrive**, bv. `C:\IctPromotie`.
3. Dubbelklik `start-scherm.cmd`. De app start en Edge opent de schermweergave schermvullend
   (sluiten met Alt+F4).
4. Wil je dat het na een herstart vanzelf opstart? Zet een snelkoppeling naar `start-scherm.cmd` in de map
   `shell:startup` (Windows-toets + R → `shell:startup`).

**Collega's laten meedoen:** zij openen `http://<naam-of-ip-van-die-pc>:5000` op hun laptop.
Daarvoor moet poort 5000 openstaan in de Windows Firewall van die pc (vraag ICT-beheer of voer als beheerder uit):

```
netsh advfirewall firewall add rule name="ICT-promotie" dir=in action=allow protocol=TCP localport=5000
```

Zet dat adres ook in `appsettings.json` bij `Scherm:BewerkUrl`, dan staat het onderaan het scherm.

## Instellingen (`appsettings.json`)

| Instelling | Betekenis |
| --- | --- |
| `Urls` | Adres en poort (standaard `http://0.0.0.0:5000`: bereikbaar vanaf het netwerk) |
| `ConnectionStrings:Promotie` | Leeg = database in `%LOCALAPPDATA%\IctPromotie\promotie.db`. Of bv. `Data Source=C:\IctPromotie\data\promotie.db` |
| `VoorbeelddataToevoegen` | Vult een lege database met voorbeelden (alles met "(voorbeeld)" kun je verwijderen) |
| `Scherm:BewerkUrl` | Adres dat onderaan het scherm staat, zodat collega's weten waar ze iets kunnen toevoegen |
| `Scherm:VerversenSeconden` | Hoe vaak het scherm zichzelf ververst |
| `Nieuws:Zoektermen` | Zoektermen voor Google Nieuws |
| `Nieuws:LokaleBronnen` | Bronnen die als *lokaal* nieuws tellen; de rest is *landelijk* |

**Let op:** zet de database niet in OneDrive. Synchroniseren terwijl de app draait kan de database beschadigen.
Maak liever af en toe een kopie van `promotie.db` als back-up.

## Mogelijke volgende stappen

- Inloggen met het CHE-account (Microsoft Entra ID), zodat niet iedereen op het netwerk alles kan wijzigen.
- Aanmeldingen voor de open dagen tonen, om te zien welke activiteiten effect hebben.
- Herinneringsmail aan mensen die bij een activiteit staan.
- Database-migraties (EF Core Migrations) zodra het datamodel gaat veranderen; nu maakt de app de database
  bij de eerste start aan met `EnsureCreated`.
