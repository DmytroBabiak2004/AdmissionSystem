using System.ComponentModel.DataAnnotations;
using AdmissionSystem.Enums;

namespace AdmissionSystem.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}
