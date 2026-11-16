using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Pages.Activiteiten;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<Activiteit> Lijst { get; private set; } = new();
    public Dictionary<int, List<string>> Conflicten { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public ActiviteitSoort? Soort { get; set; }
    [BindProperty(SupportsGet = true)] public bool Verleden { get; set; }

    public async Task OnGetAsync()
    {
        var vandaag = DateTime.Today;
        var query = db.Activiteiten.Include(a => a.Personen).Include(a => a.Materialen).AsQueryable();
        query = Verleden
            ? query.Where(a => (a.Eind ?? a.Start) < vandaag).OrderByDescending(a => a.Start)
            : query.Where(a => (a.Eind ?? a.Start) >= vandaag).OrderBy(a => a.Start);
        if (Soort is { } s) query = query.Where(a => a.Soort == s);

        Lijst = await query.ToListAsync();
        Conflicten = Planning.Conflicten(Lijst);
    }

    public async Task<IActionResult> OnPostVerwijderenAsync(int id)
    {
        if (await db.Activiteiten.FindAsync(id) is { } a)
        {
            db.Activiteiten.Remove(a);
            await db.SaveChangesAsync();
            TempData["Melding"] = $"'{a.Titel}' is verwijderd.";
        }
        return RedirectToPage(new { Soort, Verleden });
    }
}
