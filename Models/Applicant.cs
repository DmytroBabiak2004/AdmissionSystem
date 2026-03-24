using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdmissionSystem.Models;

public class Applicant
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string MiddleName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string DocumentSeriesNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string TaxCode { get; set; } = string.Empty;

    public double AverageGrade { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<ApplicantDocument> Documents { get; set; } = new List<ApplicantDocument>();

    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
}
