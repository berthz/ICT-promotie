using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IctPromotie.Pages.Personen;

public class BewerkenModel(AppDbContext db) : PageModel
{
    [BindProperty] public Persoon Persoon { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is int i)
        {
            if (await db.Personen.FindAsync(i) is not { } p) return NotFound();
            Persoon = p;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (Persoon.Id == 0) db.Personen.Add(Persoon);
        else db.Personen.Update(Persoon);
        await db.SaveChangesAsync();
        TempData["Melding"] = $"{Persoon.Naam} is opgeslagen.";
        return RedirectToPage("Index");
    }
}
