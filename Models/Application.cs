using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AdmissionSystem.Enums;

namespace AdmissionSystem.Models;

public class Application
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant Applicant { get; set; } = null!;

    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;

    public FormOfEducation FormOfEducation { get; set; }

    public EducationBasis EducationBasis { get; set; }

    public int Priority { get; set; }

    public DateTime SubmissionDate { get; set; } = DateTime.Now;

    public double CompetitiveScore { get; set; }

    public ApplicationStatus CurrentStatus { get; set; } = ApplicationStatus.Draft;

    public bool IsBudgetRecommended { get; set; }

    public bool IsContractRecommended { get; set; }

    public bool IsReserved { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();
}
