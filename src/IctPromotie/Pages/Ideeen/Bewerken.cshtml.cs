using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IctPromotie.Pages.Ideeen;

public class BewerkenModel(AppDbContext db) : PageModel
{
    [BindProperty] public Idee Idee { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (await db.Ideeen.FindAsync(id) is not { } idee) return NotFound();
        Idee = idee;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (await db.Ideeen.FindAsync(Idee.Id) is not { } bestaand) return NotFound();

        bestaand.Titel = Idee.Titel;
        bestaand.Omschrijving = Idee.Omschrijving;
        bestaand.IngediendDoor = Idee.IngediendDoor;
        bestaand.Inspanning = Idee.Inspanning;
        bestaand.Status = Idee.Status;
        await db.SaveChangesAsync();
        TempData["Melding"] = "Het idee is opgeslagen.";
        return RedirectToPage("Index");
    }
}
