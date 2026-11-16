using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Pages.Ideeen;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<Idee> Lijst { get; private set; } = new();

    /// <summary>Snel-invoerformulier bovenaan de pagina.</summary>
    [BindProperty] public Idee Nieuw { get; set; } = new();

    [BindProperty(SupportsGet = true)] public IdeeStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public Inspanning? Inspanning { get; set; }

    public async Task OnGetAsync() => await LaadAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LaadAsync();
            return Page();
        }
        Nieuw.Id = 0;
        Nieuw.Status = IdeeStatus.Nieuw;
        Nieuw.Stemmen = 0;
        Nieuw.Aangemaakt = DateTime.Now;
        db.Ideeen.Add(Nieuw);
        await db.SaveChangesAsync();
        TempData["Melding"] = "Bedankt voor je idee!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostStemAsync(int id)
    {
        await db.Ideeen.Where(i => i.Id == id).ExecuteUpdateAsync(s => s.SetProperty(i => i.Stemmen, i => i.Stemmen + 1));
        return RedirectToPage(new { Status, Inspanning });
    }

    public async Task<IActionResult> OnPostVerwijderenAsync(int id)
    {
        await db.Ideeen.Where(i => i.Id == id).ExecuteDeleteAsync();
        TempData["Melding"] = "Het idee is verwijderd.";
        return RedirectToPage(new { Status, Inspanning });
    }

    private async Task LaadAsync()
    {
        var query = db.Ideeen.AsQueryable();
        if (Status is { } s) query = query.Where(i => i.Status == s);
        else query = query.Where(i => i.Status != IdeeStatus.Afgewezen);
        if (Inspanning is { } m) query = query.Where(i => i.Inspanning == m);
        Lijst = await query.OrderByDescending(i => i.Stemmen).ThenByDescending(i => i.Aangemaakt).ToListAsync();
    }
}
