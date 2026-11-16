namespace IctPromotie.Data;

public static class Planning
{
    /// <summary>Overlappen twee activiteiten op dezelfde dag(en)?</summary>
    public static bool Overlapt(Activiteit a, Activiteit b) =>
        a.Start.Date <= b.EindOfStart.Date && b.Start.Date <= a.EindOfStart.Date;

    /// <summary>Zoekt materiaal en mensen die op dezelfde dag bij twee activiteiten zijn ingepland.
    /// Verwacht dat Personen en Materialen zijn geladen.</summary>
    public static Dictionary<int, List<string>> Conflicten(IReadOnlyList<Activiteit> lijst)
    {
        var resultaat = new Dictionary<int, List<string>>();
        void Voeg(int id, string tekst)
        {
            if (!resultaat.TryGetValue(id, out var l)) resultaat[id] = l = new();
            l.Add(tekst);
        }

        for (var i = 0; i < lijst.Count; i++)
        for (var j = i + 1; j < lijst.Count; j++)
        {
            var a = lijst[i];
            var b = lijst[j];
            if (!Overlapt(a, b)) continue;

            foreach (var m in a.Materialen.Where(m => b.Materialen.Any(x => x.Id == m.Id)))
            {
                Voeg(a.Id, $"Materiaal '{m.Naam}' is op dezelfde dag ook nodig bij '{b.Titel}'.");
                Voeg(b.Id, $"Materiaal '{m.Naam}' is op dezelfde dag ook nodig bij '{a.Titel}'.");
            }
            foreach (var p in a.Personen.Where(p => b.Personen.Any(x => x.Id == p.Id)))
            {
                Voeg(a.Id, $"{p.Naam} staat op dezelfde dag ook bij '{b.Titel}'.");
                Voeg(b.Id, $"{p.Naam} staat op dezelfde dag ook bij '{a.Titel}'.");
            }
        }
        return resultaat;
    }
}
