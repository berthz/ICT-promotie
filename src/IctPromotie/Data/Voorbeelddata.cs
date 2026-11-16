namespace IctPromotie.Data;

/// <summary>Vult een lege database met voorbeelden, zodat het scherm meteen iets laat zien.
/// Alles met "(voorbeeld)" kun je gewoon verwijderen.</summary>
public static class Voorbeelddata
{
    public static void Vul(AppDbContext db)
    {
        if (db.Activiteiten.Any() || db.Personen.Any() || db.Ideeen.Any()) return;

        var docent = new Persoon { Naam = "Docent ICT (voorbeeld)", Rol = "Docent" };
        var student = new Persoon { Naam = "Studentambassadeur (voorbeeld)", Rol = "Student" };
        var medewerker = new Persoon { Naam = "Medewerker voorlichting (voorbeeld)", Rol = "Voorlichting" };

        var banner = new Materiaal { Naam = "Banner ICT", Aantal = 2, Opslaglocatie = "Kast ICT-ruimte" };
        var laptops = new Materiaal { Naam = "Laptops met demo-game", Aantal = 4, Opslaglocatie = "Laptopkar" };
        var folders = new Materiaal { Naam = "Folders ICT", Aantal = 200, Opslaglocatie = "Kast ICT-ruimte" };
        var vr = new Materiaal { Naam = "VR-bril", Aantal = 1, Omschrijving = "Met oplader en demo", Opslaglocatie = "Kluis docentenkamer" };

        var vandaag = DateTime.Today;
        db.Activiteiten.AddRange(
            new Activiteit
            {
                Titel = "Meeloopdag ICT (voorbeeld)", Soort = ActiviteitSoort.Intern,
                Start = vandaag.AddDays(-4).AddHours(9), Eind = vandaag.AddDays(-4).AddHours(15),
                Locatie = "CHE Ede", Personen = { docent, student }, Materialen = { laptops },
            },
            new Activiteit
            {
                Titel = "Proefstudeermiddag (voorbeeld)", Soort = ActiviteitSoort.Intern,
                Start = vandaag.AddDays(3).AddHours(13), Eind = vandaag.AddDays(3).AddHours(16),
                Locatie = "CHE Ede", Personen = { docent, student }, Materialen = { laptops, vr },
            },
            new Activiteit
            {
                Titel = "Studiebeurs (voorbeeld)", Soort = ActiviteitSoort.Extern,
                Start = vandaag.AddDays(9).AddHours(10), Eind = vandaag.AddDays(10).AddHours(16),
                Locatie = "Jaarbeurs Utrecht", Omschrijving = "Stand samen met voorlichting.",
                Personen = { medewerker, student }, Materialen = { banner, folders, vr },
            },
            new Activiteit
            {
                Titel = "Gastles op middelbare school (voorbeeld)", Soort = ActiviteitSoort.Extern,
                Start = vandaag.AddDays(10).AddHours(11), Eind = vandaag.AddDays(10).AddHours(12),
                Locatie = "Middelbare school in de regio", Personen = { docent }, Materialen = { vr },
            },
            new Activiteit
            {
                Titel = "Open avond (voorbeeld)", Soort = ActiviteitSoort.OpenDag,
                Start = vandaag.AddDays(17).AddHours(18).AddMinutes(30), Eind = vandaag.AddDays(17).AddHours(21),
                Locatie = "CHE Ede", Personen = { docent, student, medewerker }, Materialen = { banner, laptops, folders },
            });

        db.Ideeen.AddRange(
            new Idee { Titel = "Studenten mailen hun oude middelbare school", Omschrijving = "Kort mailtje met uitnodiging voor de open dag, wij leveren de tekst.", Inspanning = Inspanning.Laag, Stemmen = 4 },
            new Idee { Titel = "Elke docent deelt de open dag op LinkedIn", Omschrijving = "In de week voor de open dag, met een kant-en-klare post en afbeelding.", Inspanning = Inspanning.Laag, Stemmen = 3 },
            new Idee { Titel = "Korte video van een projectdag voor Instagram", Inspanning = Inspanning.Middel, Stemmen = 2 },
            new Idee { Titel = "QR-code naar aanmelden open dag op de schermen in de hal", Inspanning = Inspanning.Laag, Stemmen = 1 });

        db.SaveChanges();
    }
}
