using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Pages.Nieuws;

public class BewerkenModel(AppDbContext db) : PageModel
{
    [BindProperty] public NieuwsItem Item { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is int i)
        {
            if (await db.Nieuws.FindAsync(i) is not { } n) return NotFound();
            Item = n;
        }
        else
        {
            Item = new NieuwsItem { Bereik = NieuwsBereik.Lokaal };
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (await db.Nieuws.AnyAsync(n => n.Url == Item.Url && n.Id != Item.Id))
            ModelState.AddModelError("Item.Url", "Dit bericht staat er al in.");
        if (!ModelState.IsValid) return Page();

        if (Item.Id == 0)
        {
            Item.Automatisch = false;
            db.Nieuws.Add(Item);
        }
        else
        {
            if (await db.Nieuws.FindAsync(Item.Id) is not { } bestaand) return NotFound();
            bestaand.Titel = Item.Titel;
            bestaand.Url = Item.Url;
            bestaand.Bron = Item.Bron;
            bestaand.Gepubliceerd = Item.Gepubliceerd;
            bestaand.Bereik = Item.Bereik;
            bestaand.Verborgen = Item.Verborgen;
        }
        await db.SaveChangesAsync();
        TempData["Melding"] = "Het nieuwsbericht is opgeslagen.";
        return RedirectToPage("Index");
    }
}
