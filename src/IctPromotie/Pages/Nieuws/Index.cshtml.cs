using IctPromotie.Data;
using IctPromotie.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Pages.Nieuws;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<NieuwsItem> Lokaal { get; private set; } = new();
    public List<NieuwsItem> Landelijk { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public bool ToonVerborgen { get; set; }

    public async Task OnGetAsync()
    {
        var alles = await db.Nieuws
            .Where(n => ToonVerborgen || !n.Verborgen)
            .OrderByDescending(n => n.Gepubliceerd)
            .Take(300)
            .ToListAsync();
        Lokaal = alles.Where(n => n.Bereik == NieuwsBereik.Lokaal).ToList();
        Landelijk = alles.Where(n => n.Bereik == NieuwsBereik.Landelijk).ToList();
    }

    public async Task<IActionResult> OnPostOphalenAsync([FromServices] NieuwsOphaler ophaler)
    {
        var aantal = await ophaler.HaalOpAsync(HttpContext.RequestAborted);
        TempData["Melding"] = aantal == 0 ? "Geen nieuwe berichten gevonden." : $"{aantal} nieuwe bericht(en) opgehaald.";
        return RedirectToPage(new { ToonVerborgen });
    }

    /// <summary>Automatisch opgehaalde berichten worden verborgen (anders komen ze terug);
    /// handmatige berichten worden echt verwijderd.</summary>
    public async Task<IActionResult> OnPostVerwijderenAsync(int id)
    {
        if (await db.Nieuws.FindAsync(id) is { } n)
        {
            if (n.Automatisch) n.Verborgen = true;
            else db.Nieuws.Remove(n);
            await db.SaveChangesAsync();
            TempData["Melding"] = n.Automatisch ? "Het bericht is verborgen." : "Het bericht is verwijderd.";
        }
        return RedirectToPage(new { ToonVerborgen });
    }

    public async Task<IActionResult> OnPostTerugzettenAsync(int id)
    {
        await db.Nieuws.Where(n => n.Id == id).ExecuteUpdateAsync(s => s.SetProperty(n => n.Verborgen, false));
        return RedirectToPage(new { ToonVerborgen });
    }

    public async Task<IActionResult> OnPostWisselBereikAsync(int id)
    {
        if (await db.Nieuws.FindAsync(id) is { } n)
        {
            n.Bereik = n.Bereik == NieuwsBereik.Lokaal ? NieuwsBereik.Landelijk : NieuwsBereik.Lokaal;
            await db.SaveChangesAsync();
        }
        return RedirectToPage(new { ToonVerborgen });
    }
}
