using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdmissionSystem.Data;
using AdmissionSystem.Enums;
using AdmissionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Services;

public class ApplicationService
{
    private readonly AppDbContext _context;

    // Більше не приймає DocumentService — використовує той самий DbContext
    public ApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Application>> GetAllAsync()
    {
        return await _context.Applications
            .Include(a => a.Applicant)
            .Include(a => a.Specialty)
            .Include(a => a.StatusHistory)
            .OrderByDescending(a => a.SubmissionDate)
            .ToListAsync();
    }

    public async Task<List<Application>> GetByApplicantAsync(int applicantId)
    {
        return await _context.Applications
            .Include(a => a.Specialty)
            .Include(a => a.StatusHistory)
            .Where(a => a.ApplicantId == applicantId)
            .OrderBy(a => a.Priority)
            .ToListAsync();
    }

    public async Task<Application?> GetByIdAsync(int id)
    {
        return await _context.Applications
            .Include(a => a.Applicant)
            .Include(a => a.Specialty)
            .Include(a => a.StatusHistory.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Application> CreateAsync(Application application, string changedBy)
    {
        if (application.ApplicantId == 0)
            throw new InvalidOperationException("Необхідно вказати абітурієнта.");
        if (application.SpecialtyId == 0)
            throw new InvalidOperationException("Необхідно вказати спеціальність.");
        if (application.Priority <= 0)
            throw new InvalidOperationException("Пріоритет має бути додатним числом.");
        if (application.CompetitiveScore < 0)
            throw new InvalidOperationException("Конкурсний бал не може бути від'ємним.");

        application.SubmissionDate = DateTime.Now;
        application.CurrentStatus = ApplicationStatus.Draft;

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        await AddStatusHistoryAsync(
            application.Id,
            ApplicationStatus.Draft,
            ApplicationStatus.Draft,
            changedBy,
            "Заяву створено");

        return application;
    }

    public async Task UpdateAsync(Application application)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Error)> ChangeStatusAsync(
        int applicationId, ApplicationStatus newStatus, string changedBy, string comment = "")
    {
        var app = await _context.Applications
            .Include(a => a.Applicant)
                .ThenInclude(ap => ap.Documents)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (app == null) return (false, "Заяву не знайдено.");

        var (valid, error) = ValidateStatusTransition(app, newStatus);
        if (!valid) return (false, error);

        var oldStatus = app.CurrentStatus;
        app.CurrentStatus = newStatus;
        await _context.SaveChangesAsync();

        await AddStatusHistoryAsync(applicationId, oldStatus, newStatus, changedBy, comment);

        return (true, string.Empty);
    }

    private (bool Valid, string Error) ValidateStatusTransition(
        Application app, ApplicationStatus newStatus)
    {
        var current = app.CurrentStatus;

        if (current == ApplicationStatus.Draft && newStatus == ApplicationStatus.Enrolled)
            return (false, "Неможливо зарахувати заяву напряму зі статусу 'Чернетка'.");

        if (newStatus == ApplicationStatus.AdmittedToCompetition &&
            current != ApplicationStatus.DocumentsConfirmed)
            return (false, "Для допуску до конкурсу документи мають бути підтверджені.");

        if (newStatus == ApplicationStatus.DocumentsConfirmed)
        {
            var requiredTypes = new[]
            {
                DocumentType.Passport,
                DocumentType.EducationDocument,
                DocumentType.NmtCertificate
            };
            var docs = app.Applicant?.Documents ?? new List<ApplicantDocument>();
            foreach (var req in requiredTypes)
            {
                if (!docs.Any(d => d.DocumentType == req && d.IsProvided && d.IsVerified))
                    return (false, $"Документ типу '{req.ToDisplayString()}' не підтверджено.");
            }
        }

        return (true, string.Empty);
    }

    private async Task AddStatusHistoryAsync(
        int applicationId, ApplicationStatus oldStatus,
        ApplicationStatus newStatus, string changedBy, string comment)
    {
        _context.StatusHistories.Add(new ApplicationStatusHistory
        {
            ApplicationId = applicationId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedBy,
            ChangedAt = DateTime.Now,
            Comment = comment
        });
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var app = await _context.Applications.FindAsync(id);
        if (app != null)
        {
            _context.Applications.Remove(app);
            await _context.SaveChangesAsync();
        }
    }
}