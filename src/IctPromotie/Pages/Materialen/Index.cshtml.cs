using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Pages.Materialen;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<(Materiaal Materiaal, Activiteit? Volgende)> Lijst { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var vandaag = DateTime.Today;
        var rijen = await db.Materialen
            .OrderBy(m => m.Naam)
            .Select(m => new
            {
                m,
                Volgende = m.Activiteiten.Where(a => (a.Eind ?? a.Start) >= vandaag).OrderBy(a => a.Start).FirstOrDefault(),
            })
            .ToListAsync();
        Lijst = rijen.Select(r => (r.m, r.Volgende)).ToList();
    }

    public async Task<IActionResult> OnPostVerwijderenAsync(int id)
    {
        if (await db.Materialen.FindAsync(id) is { } m)
        {
            db.Materialen.Remove(m);
            await db.SaveChangesAsync();
            TempData["Melding"] = $"'{m.Naam}' is verwijderd.";
        }
        return RedirectToPage();
    }
}
