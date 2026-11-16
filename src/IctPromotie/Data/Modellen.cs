using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace IctPromotie.Data;

public enum ActiviteitSoort
{
    [Display(Name = "Interne promotie")] Intern,
    [Display(Name = "Externe promotie")] Extern,
    [Display(Name = "Open dag / open avond")] OpenDag,
}

public enum NieuwsBereik
{
    Landelijk,
    Lokaal,
}

public enum Inspanning
{
    [Display(Name = "Weinig moeite")] Laag,
    [Display(Name = "Gemiddeld")] Middel,
    [Display(Name = "Veel moeite")] Hoog,
}

public enum IdeeStatus
{
    [Display(Name = "Nieuw")] Nieuw,
    [Display(Name = "Gaan we doen")] Gepland,
    [Display(Name = "Gedaan")] Gedaan,
    [Display(Name = "Niet haalbaar")] Afgewezen,
}

/// <summary>Een promotieactiviteit: intern (op de CHE), extern (op locatie) of een open dag/avond.</summary>
public class Activiteit
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Geef de activiteit een titel.")]
    [StringLength(150)]
    public string Titel { get; set; } = "";

    public ActiviteitSoort Soort { get; set; }

    [Display(Name = "Begint op")]
    [Required(ErrorMessage = "Vul in wanneer de activiteit begint.")]
    public DateTime Start { get; set; }

    [Display(Name = "Eindigt op")]
    public DateTime? Eind { get; set; }

    [StringLength(200)]
    public string? Locatie { get; set; }

    [StringLength(2000)]
    public string? Omschrijving { get; set; }

    public List<Persoon> Personen { get; set; } = new();
    public List<Materiaal> Materialen { get; set; } = new();

    [NotMapped]
    public DateTime EindOfStart => Eind ?? Start;

    public bool ValtOp(DateOnly dag) =>
        DateOnly.FromDateTime(Start) <= dag && dag <= DateOnly.FromDateTime(EindOfStart);

    /// <summary>Korte, leesbare weergave van datum en tijd, bv. "di 18 nov 19:00–21:00".</summary>
    [NotMapped]
    public string Wanneer
    {
        get
        {
            var tijd = Start.TimeOfDay == TimeSpan.Zero ? "" : $" {Start:HH:mm}";
            if (Eind is not DateTime eind) return $"{Start:ddd d MMM}{tijd}";
            if (eind.Date == Start.Date) return $"{Start:ddd d MMM}{tijd}–{eind:HH:mm}";
            return $"{Start:ddd d MMM} t/m {eind:ddd d MMM}";
        }
    }
}

/// <summary>Iemand die activiteiten geeft: docent, student-ambassadeur, medewerker.</summary>
public class Persoon
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vul een naam in.")]
    [StringLength(100)]
    public string Naam { get; set; } = "";

    [StringLength(100)]
    public string? Rol { get; set; }

    [EmailAddress(ErrorMessage = "Dit is geen geldig e-mailadres.")]
    [StringLength(150)]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    public List<Activiteit> Activiteiten { get; set; } = new();
}

/// <summary>Promotiemateriaal: banner, laptops, folders, VR-bril, ...</summary>
public class Materiaal
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vul een naam in.")]
    [StringLength(100)]
    public string Naam { get; set; } = "";

    [StringLength(500)]
    public string? Omschrijving { get; set; }

    [Range(0, 10000, ErrorMessage = "Vul een aantal tussen 0 en 10000 in.")]
    public int Aantal { get; set; } = 1;

    [StringLength(150)]
    [Display(Name = "Waar ligt het?")]
    public string? Opslaglocatie { get; set; }

    public List<Activiteit> Activiteiten { get; set; } = new();
}

/// <summary>Een nieuwsbericht waarin de ICT-opleiding van de CHE voorkomt.</summary>
public class NieuwsItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vul een titel in.")]
    [StringLength(300)]
    public string Titel { get; set; } = "";

    [Required(ErrorMessage = "Vul de link naar het artikel in.")]
    [Url(ErrorMessage = "Dit is geen geldige link.")]
    [StringLength(1000)]
    [Display(Name = "Link")]
    public string Url { get; set; } = "";

    [StringLength(150)]
    public string? Bron { get; set; }

    [Display(Name = "Datum")]
    [DataType(DataType.Date)]
    public DateTime Gepubliceerd { get; set; } = DateTime.Today;

    public NieuwsBereik Bereik { get; set; }

    /// <summary>True als het bericht automatisch via Google Nieuws is opgehaald.</summary>
    public bool Automatisch { get; set; }

    /// <summary>Verborgen berichten worden niet getoond (en niet opnieuw opgehaald).</summary>
    public bool Verborgen { get; set; }
}

/// <summary>Een idee om de opleiding met weinig moeite te promoten.</summary>
public class Idee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Beschrijf je idee in een paar woorden.")]
    [StringLength(150)]
    [Display(Name = "Idee")]
    public string Titel { get; set; } = "";

    [StringLength(2000)]
    [Display(Name = "Toelichting")]
    public string? Omschrijving { get; set; }

    [StringLength(100)]
    [Display(Name = "Van")]
    public string? IngediendDoor { get; set; }

    public Inspanning Inspanning { get; set; } = Inspanning.Laag;

    public IdeeStatus Status { get; set; } = IdeeStatus.Nieuw;

    public int Stemmen { get; set; }

    public DateTime Aangemaakt { get; set; } = DateTime.Now;
}

public static class EnumNamen
{
    /// <summary>Geeft de Nederlandse weergavenaam uit het [Display]-attribuut.</summary>
    public static string Naam(this Enum waarde) =>
        waarde.GetType().GetField(waarde.ToString())?.GetCustomAttribute<DisplayAttribute>()?.GetName()
        ?? waarde.ToString();
}
