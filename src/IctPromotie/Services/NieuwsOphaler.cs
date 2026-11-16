using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using IctPromotie.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IctPromotie.Services;

public class NieuwsOpties
{
    public bool Automatisch { get; set; } = true;
    public double IntervalUren { get; set; } = 3;
    public List<string> Zoektermen { get; set; } = new();
    public List<string> LokaleBronnen { get; set; } = new();
}

/// <summary>Haalt nieuws op via de RSS-feed van Google Nieuws en slaat nieuwe berichten op.</summary>
public class NieuwsOphaler(HttpClient http, AppDbContext db, IOptions<NieuwsOpties> opties, ILogger<NieuwsOphaler> log)
{
    public async Task<int> HaalOpAsync(CancellationToken ct = default)
    {
        var o = opties.Value;
        var bekend = (await db.Nieuws.Select(n => n.Url).ToListAsync(ct)).ToHashSet();
        var nieuw = 0;

        foreach (var term in o.Zoektermen.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var url = $"https://news.google.com/rss/search?q={Uri.EscapeDataString(term)}&hl=nl&gl=NL&ceid=NL:nl";
            try
            {
                await using var stream = await http.GetStreamAsync(url, ct);
                var doc = await XDocument.LoadAsync(stream, LoadOptions.None, ct);

                foreach (var item in doc.Descendants("item"))
                {
                    var link = ((string?)item.Element("link"))?.Trim();
                    var titel = ((string?)item.Element("title"))?.Trim();
                    if (string.IsNullOrEmpty(link) || string.IsNullOrEmpty(titel) || !bekend.Add(link)) continue;

                    var bron = ((string?)item.Element("source"))?.Trim() ?? "";
                    // Google zet de bron achter de titel: "Titel - Bron".
                    if (bron.Length > 0 && titel.EndsWith(" - " + bron)) titel = titel[..^(bron.Length + 3)];

                    var datum = DateTimeOffset.TryParse((string?)item.Element("pubDate"), CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var d) ? d.LocalDateTime : DateTime.Now;

                    db.Nieuws.Add(new NieuwsItem
                    {
                        Titel = titel.Length > 300 ? titel[..300] : titel,
                        Url = link,
                        Bron = bron.Length > 150 ? bron[..150] : bron,
                        Gepubliceerd = datum,
                        Bereik = o.LokaleBronnen.Any(l => bron.Contains(l, StringComparison.OrdinalIgnoreCase))
                            ? NieuwsBereik.Lokaal : NieuwsBereik.Landelijk,
                        Automatisch = true,
                    });
                    nieuw++;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or XmlException or TaskCanceledException && !ct.IsCancellationRequested)
            {
                log.LogWarning(ex, "Nieuws ophalen voor zoekterm '{Term}' is mislukt", term);
            }
        }

        await db.SaveChangesAsync(ct);
        return nieuw;
    }
}

/// <summary>Haalt op de achtergrond periodiek nieuws op.</summary>
public class NieuwsAchtergrondDienst(IServiceScopeFactory scopes, IOptions<NieuwsOpties> opties, ILogger<NieuwsAchtergrondDienst> log)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!opties.Value.Automatisch) return;

        await Task.Delay(TimeSpan.FromSeconds(10), ct);
        using var timer = new PeriodicTimer(TimeSpan.FromHours(Math.Max(0.25, opties.Value.IntervalUren)));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var aantal = await scope.ServiceProvider.GetRequiredService<NieuwsOphaler>().HaalOpAsync(ct);
                log.LogInformation("{Aantal} nieuwe nieuwsberichten opgehaald", aantal);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                log.LogError(ex, "Fout bij automatisch ophalen van nieuws");
            }
        } while (await timer.WaitForNextTickAsync(ct));
    }
}
