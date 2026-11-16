using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Pages.Activiteiten;

/// <summary>Toevoegen (zonder id) en wijzigen (met id) van een activiteit.</summary>
public class BewerkenModel(AppDbContext db) : PageModel
{
    [BindProperty] public Activiteit Activiteit { get; set; } = new();
    [BindProperty] public List<int> PersoonIds { get; set; } = new();
    [BindProperty] public List<int> MateriaalIds { get; set; } = new();

    public List<Persoon> AllePersonen { get; private set; } = new();
    public List<Materiaal> AlleMaterialen { get; private set; } = new();
    public List<string> Waarschuwingen { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id, DateOnly? datum)
    {
        if (id is int i)
        {
            var a = await db.Activiteiten.Include(x => x.Personen).Include(x => x.Materialen).FirstOrDefaultAsync(x => x.Id == i);
            if (a is null) return NotFound();
            Activiteit = a;
            PersoonIds = a.Personen.Select(p => p.Id).ToList();
            MateriaalIds = a.Materialen.Select(m => m.Id).ToList();
            await ZoekConflictenAsync(a);
        }
        else
        {
            var dag = datum ?? DateOnly.FromDateTime(DateTime.Today);
            Activiteit = new Activiteit { Start = dag.ToDateTime(new TimeOnly(9, 0)) };
        }
        await LaadKeuzesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Activiteit.Eind is DateTime eind && eind < Activiteit.Start)
            ModelState.AddModelError("Activiteit.Eind", "Het einde ligt vóór het begin.");
        if (!ModelState.IsValid)
        {
            await LaadKeuzesAsync();
            return Page();
        }

        Activiteit doel;
        if (Activiteit.Id == 0)
        {
            doel = Activiteit;
            db.Activiteiten.Add(doel);
        }
        else
        {
            var bestaand = await db.Activiteiten.Include(x => x.Personen).Include(x => x.Materialen)
                .FirstOrDefaultAsync(x => x.Id == Activiteit.Id);
            if (bestaand is null) return NotFound();
            db.Entry(bestaand).CurrentValues.SetValues(Activiteit);
            doel = bestaand;
        }

        doel.Personen.Clear();
        doel.Personen.AddRange(await db.Personen.Where(p => PersoonIds.Contains(p.Id)).ToListAsync());
        doel.Materialen.Clear();
        doel.Materialen.AddRange(await db.Materialen.Where(m => MateriaalIds.Contains(m.Id)).ToListAsync());
        await db.SaveChangesAsync();

        await ZoekConflictenAsync(doel);
        TempData["Melding"] = Waarschuwingen.Count == 0
            ? $"'{doel.Titel}' is opgeslagen."
            : $"'{doel.Titel}' is opgeslagen. Let op: {string.Join(" ", Waarschuwingen)}";
        return RedirectToPage("/Index", new { jaar = doel.Start.Year, maand = doel.Start.Month });
    }

    public async Task<IActionResult> OnPostVerwijderenAsync(int id)
    {
        if (await db.Activiteiten.FindAsync(id) is { } a)
        {
            db.Activiteiten.Remove(a);
            await db.SaveChangesAsync();
            TempData["Melding"] = $"'{a.Titel}' is verwijderd.";
        }
        return RedirectToPage("/Index");
    }

    private async Task LaadKeuzesAsync()
    {
        AllePersonen = await db.Personen.OrderBy(p => p.Naam).ToListAsync();
        AlleMaterialen = await db.Materialen.OrderBy(m => m.Naam).ToListAsync();
    }

    /// <summary>Waarschuwt als mensen of materiaal op dezelfde dag al bij een andere activiteit staan.</summary>
    private async Task ZoekConflictenAsync(Activiteit a)
    {
        var van = a.Start.Date;
        var tot = a.EindOfStart.Date.AddDays(1);
        var anderen = await db.Activiteiten.Include(x => x.Personen).Include(x => x.Materialen)
            .Where(x => x.Id != a.Id && x.Start < tot && (x.Eind ?? x.Start) >= van)
            .ToListAsync();
        anderen.Insert(0, a);
        Waarschuwingen = Planning.Conflicten(anderen).GetValueOrDefault(a.Id) ?? new();
    }
}
