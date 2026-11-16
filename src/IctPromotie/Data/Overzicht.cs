using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Data;

public class KalenderModel
{
    public required DateOnly EersteDag { get; init; }
    public required List<Activiteit> Activiteiten { get; init; }
    public bool Scherm { get; init; }

    /// <summary>Maandag van de week waarin de maand begint.</summary>
    public DateOnly GridStart => EersteDag.AddDays(-(((int)EersteDag.DayOfWeek + 6) % 7));
    public DateOnly LaatsteDag => EersteDag.AddMonths(1).AddDays(-1);
    public int AantalWeken => (LaatsteDag.DayNumber - GridStart.DayNumber) / 7 + 1;
    public DateOnly GridEind => GridStart.AddDays(AantalWeken * 7 - 1);
}

/// <summary>Alles wat de homepage en het scherm nodig hebben.</summary>
public class Overzicht
{
    public required KalenderModel Kalender { get; init; }
    public required List<Activiteit> Komende { get; init; }
    public Activiteit? VolgendeOpenDag { get; init; }
    public required List<NieuwsItem> Nieuws { get; init; }
    public required List<Idee> Ideeen { get; init; }

    public static async Task<Overzicht> LaadAsync(AppDbContext db, int? jaar, int? maand, bool scherm)
    {
        var vandaag = DateTime.Today;
        var j = Math.Clamp(jaar ?? vandaag.Year, 2000, 2100);
        var eerste = new DateOnly(j, 1, 1).AddMonths(Math.Clamp(maand ?? vandaag.Month, 1, 12) - 1);

        var kalender = new KalenderModel { EersteDag = eerste, Activiteiten = new(), Scherm = scherm };
        var van = kalender.GridStart.ToDateTime(TimeOnly.MinValue);
        var tot = kalender.GridEind.AddDays(1).ToDateTime(TimeOnly.MinValue);
        kalender.Activiteiten.AddRange(await db.Activiteiten
            .Where(a => a.Start < tot && (a.Eind ?? a.Start) >= van)
            .OrderBy(a => a.Start)
            .ToListAsync());

        return new Overzicht
        {
            Kalender = kalender,
            Komende = await db.Activiteiten
                .Include(a => a.Personen)
                .Where(a => (a.Eind ?? a.Start) >= vandaag)
                .OrderBy(a => a.Start)
                .Take(scherm ? 4 : 6)
                .ToListAsync(),
            VolgendeOpenDag = await db.Activiteiten
                .Where(a => a.Soort == ActiviteitSoort.OpenDag && a.Start >= vandaag)
                .OrderBy(a => a.Start)
                .FirstOrDefaultAsync(),
            Nieuws = await db.Nieuws
                .Where(n => !n.Verborgen)
                .OrderByDescending(n => n.Gepubliceerd)
                .Take(scherm ? 4 : 6)
                .ToListAsync(),
            Ideeen = await db.Ideeen
                .Where(i => i.Status == IdeeStatus.Nieuw || i.Status == IdeeStatus.Gepland)
                .OrderByDescending(i => i.Stemmen).ThenByDescending(i => i.Aangemaakt)
                .Take(scherm ? 2 : 4)
                .ToListAsync(),
        };
    }
}
