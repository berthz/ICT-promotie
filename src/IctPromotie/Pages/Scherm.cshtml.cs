using IctPromotie.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IctPromotie.Pages;

/// <summary>Weergave voor het grote scherm in de ICT-ruimte: geen menu, ververst zichzelf.</summary>
public class SchermModel(AppDbContext db, IConfiguration config) : PageModel
{
    public Overzicht Overzicht { get; private set; } = null!;
    public int VerversenSeconden { get; private set; }
    public string BewerkUrl { get; private set; } = "";

    public async Task OnGetAsync()
    {
        Overzicht = await Overzicht.LaadAsync(db, null, null, scherm: true);
        VerversenSeconden = Math.Max(30, config.GetValue("Scherm:VerversenSeconden", 300));
        BewerkUrl = config["Scherm:BewerkUrl"] ?? "";
    }
}
