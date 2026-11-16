using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IctPromotie.Pages.Materialen;

public class BewerkenModel(AppDbContext db) : PageModel
{
    [BindProperty] public Materiaal Materiaal { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is int i)
        {
            if (await db.Materialen.FindAsync(i) is not { } m) return NotFound();
            Materiaal = m;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (Materiaal.Id == 0) db.Materialen.Add(Materiaal);
        else db.Materialen.Update(Materiaal);
        await db.SaveChangesAsync();
        TempData["Melding"] = $"'{Materiaal.Naam}' is opgeslagen.";
        return RedirectToPage("Index");
    }
}
