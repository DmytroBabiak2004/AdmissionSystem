using System;
using System.ComponentModel.DataAnnotations;
using AdmissionSystem.Enums;

namespace AdmissionSystem.Models;

public class ApplicationStatusHistory
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    public ApplicationStatus OldStatus { get; set; }

    public ApplicationStatus NewStatus { get; set; }

    [MaxLength(200)]
    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
}
