using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Pages.Personen;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<(Persoon Persoon, int Komend)> Lijst { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var vandaag = DateTime.Today;
        var rijen = await db.Personen
            .OrderBy(p => p.Naam)
            .Select(p => new { p, Komend = p.Activiteiten.Count(a => (a.Eind ?? a.Start) >= vandaag) })
            .ToListAsync();
        Lijst = rijen.Select(r => (r.p, r.Komend)).ToList();
    }

    public async Task<IActionResult> OnPostVerwijderenAsync(int id)
    {
        if (await db.Personen.FindAsync(id) is { } p)
        {
            db.Personen.Remove(p);
            await db.SaveChangesAsync();
            TempData["Melding"] = $"{p.Naam} is verwijderd.";
        }
        return RedirectToPage();
    }
}
