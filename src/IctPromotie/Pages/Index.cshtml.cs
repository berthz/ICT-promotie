using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IctPromotie.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    public Overzicht Overzicht { get; private set; } = null!;

    public async Task OnGetAsync(int? jaar, int? maand)
    {
        Overzicht = await Overzicht.LaadAsync(db, jaar, maand, scherm: false);
    }
}
