using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdmissionSystem.Models;

public class Specialty
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Faculty { get; set; } = string.Empty;

    public int BudgetPlaces { get; set; }

    public int ContractPlaces { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();

    public override string ToString() => $"{Code} - {Name}";
}
