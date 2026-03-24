using System;
using System.ComponentModel.DataAnnotations;
using AdmissionSystem.Enums;

namespace AdmissionSystem.Models;

public class ApplicantDocument
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant Applicant { get; set; } = null!;

    public DocumentType DocumentType { get; set; }

    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    public bool IsProvided { get; set; }

    public bool IsVerified { get; set; }

    public DateTime? UploadedAt { get; set; }

    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
}
