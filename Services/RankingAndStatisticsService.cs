using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdmissionSystem.Data;
using AdmissionSystem.Enums;
using AdmissionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Services;

public class RankingService
{
    private readonly AppDbContext _context;

    public RankingService(AppDbContext context) => _context = context;

    public async Task<List<RankingEntry>> GetRankingAsync(int specialtyId)
    {
        var specialty = await _context.Specialties.FindAsync(specialtyId)
            ?? throw new System.Exception("Спеціальність не знайдено.");

        var apps = await _context.Applications
            .Include(a => a.Applicant)
            .Where(a => a.SpecialtyId == specialtyId &&
                        a.CurrentStatus == ApplicationStatus.AdmittedToCompetition)
            .OrderByDescending(a => a.CompetitiveScore)
            .ThenBy(a => a.SubmissionDate)
            .ToListAsync();

        var result = new List<RankingEntry>();
        for (int i = 0; i < apps.Count; i++)
        {
            var app = apps[i];
            string recommendation;
            if (i < specialty.BudgetPlaces)
                recommendation = "Бюджет";
            else if (i < specialty.BudgetPlaces + specialty.ContractPlaces)
                recommendation = "Контракт";
            else
                recommendation = "Резерв";

            result.Add(new RankingEntry
            {
                Rank = i + 1,
                Application = app,
                FullName = app.Applicant.FullName,
                CompetitiveScore = app.CompetitiveScore,
                Priority = app.Priority,
                Recommendation = recommendation
            });
        }
        return result;
    }

    public async Task RecalculateAsync(int specialtyId)
    {
        var specialty = await _context.Specialties.FindAsync(specialtyId)
            ?? throw new System.Exception("Спеціальність не знайдено.");

        var apps = await _context.Applications
            .Where(a => a.SpecialtyId == specialtyId)
            .ToListAsync();

        // Reset flags
        foreach (var a in apps)
        {
            a.IsBudgetRecommended = false;
            a.IsContractRecommended = false;
            a.IsReserved = false;
        }

        var competing = apps
            .Where(a => a.CurrentStatus == ApplicationStatus.AdmittedToCompetition)
            .OrderByDescending(a => a.CompetitiveScore)
            .ThenBy(a => a.SubmissionDate)
            .ToList();

        for (int i = 0; i < competing.Count; i++)
        {
            if (i < specialty.BudgetPlaces)
                competing[i].IsBudgetRecommended = true;
            else if (i < specialty.BudgetPlaces + specialty.ContractPlaces)
                competing[i].IsContractRecommended = true;
            else
                competing[i].IsReserved = true;
        }

        await _context.SaveChangesAsync();
    }
}

public class RankingEntry
{
    public int Rank { get; set; }
    public Application Application { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public double CompetitiveScore { get; set; }
    public int Priority { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class StatisticsService
{
    private readonly AppDbContext _context;

    public StatisticsService(AppDbContext context) => _context = context;

    public async Task<CampaignStatistics> GetStatisticsAsync()
    {
        var stats = new CampaignStatistics
        {
            TotalApplicants = await _context.Applicants.CountAsync(),
            TotalApplications = await _context.Applications.CountAsync(),
            BudgetRecommended = await _context.Applications.CountAsync(a => a.IsBudgetRecommended),
            ContractRecommended = await _context.Applications.CountAsync(a => a.IsContractRecommended),
            Reserved = await _context.Applications.CountAsync(a => a.IsReserved)
        };

        stats.BySpecialty = await _context.Applications
            .Include(a => a.Specialty)
            .GroupBy(a => a.Specialty.Name)
            .Select(g => new SpecialtyStats { SpecialtyName = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.ByStatus = await _context.Applications
            .GroupBy(a => a.CurrentStatus)
            .Select(g => new StatusStats { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats;
    }
}

public class CampaignStatistics
{
    public int TotalApplicants { get; set; }
    public int TotalApplications { get; set; }
    public int BudgetRecommended { get; set; }
    public int ContractRecommended { get; set; }
    public int Reserved { get; set; }
    public List<SpecialtyStats> BySpecialty { get; set; } = new();
    public List<StatusStats> ByStatus { get; set; } = new();
}

public class SpecialtyStats
{
    public string SpecialtyName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class StatusStats
{
    public ApplicationStatus Status { get; set; }
    public int Count { get; set; }
    public string StatusName => Status.ToDisplayString();
}
