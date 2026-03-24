using AdmissionSystem.Enums;
using AdmissionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Data;

public class AppDbContext : DbContext
{
    public DbSet<Applicant> Applicants { get; set; }
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<ApplicationStatusHistory> StatusHistories { get; set; }
    public DbSet<ApplicantDocument> Documents { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Specialty seed data
        modelBuilder.Entity<Specialty>().HasData(
            new Specialty
            {
                Id = 1,
                Name = "Комп'ютерні науки",
                Code = "122",
                Faculty = "Факультет інформаційних технологій",
                BudgetPlaces = 25,
                ContractPlaces = 50,
                Description = "Підготовка фахівців у сфері комп'ютерних наук та програмування",
                IsActive = true
            },
            new Specialty
            {
                Id = 2,
                Name = "Програмна інженерія",
                Code = "121",
                Faculty = "Факультет інформаційних технологій",
                BudgetPlaces = 20,
                ContractPlaces = 40,
                Description = "Розробка програмного забезпечення та інженерія вимог",
                IsActive = true
            },
            new Specialty
            {
                Id = 3,
                Name = "Кібербезпека",
                Code = "125",
                Faculty = "Факультет інформаційних технологій",
                BudgetPlaces = 15,
                ContractPlaces = 30,
                Description = "Захист інформаційних систем та кіберпросторів",
                IsActive = true
            },
            new Specialty
            {
                Id = 4,
                Name = "Фінанси, банківська справа та страхування",
                Code = "072",
                Faculty = "Економічний факультет",
                BudgetPlaces = 30,
                ContractPlaces = 60,
                Description = "Підготовка фінансових аналітиків та банківських спеціалістів",
                IsActive = true
            },
            new Specialty
            {
                Id = 5,
                Name = "Право",
                Code = "081",
                Faculty = "Юридичний факультет",
                BudgetPlaces = 20,
                ContractPlaces = 80,
                Description = "Підготовка юристів широкого профілю",
                IsActive = true
            }
        );

        // User seed data
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", FullName = "Адміністратор системи", Role = UserRole.Administrator },
            new User { Id = 2, Username = "operator1", FullName = "Іваненко Марія Іванівна", Role = UserRole.Operator },
            new User { Id = 3, Username = "reviewer1", FullName = "Петренко Олег Сергійович", Role = UserRole.Reviewer }
        );

        // Configure relationships
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Applicant)
            .WithMany(ap => ap.Applications)
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Application>()
            .HasOne(a => a.Specialty)
            .WithMany(s => s.Applications)
            .HasForeignKey(a => a.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationStatusHistory>()
            .HasOne(h => h.Application)
            .WithMany(a => a.StatusHistory)
            .HasForeignKey(h => h.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicantDocument>()
            .HasOne(d => d.Applicant)
            .WithMany(a => a.Documents)
            .HasForeignKey(d => d.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=AdmissionSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}
