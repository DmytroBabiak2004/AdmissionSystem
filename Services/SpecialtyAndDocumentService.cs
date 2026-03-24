using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdmissionSystem.Data;
using AdmissionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Services;

public class SpecialtyService
{
    private readonly AppDbContext _context;

    public SpecialtyService(AppDbContext context) => _context = context;

    public async Task<List<Specialty>> GetAllAsync()
        => await _context.Specialties
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .ToListAsync();

    public async Task<List<Specialty>> GetActiveAsync()
        => await _context.Specialties
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Code)
            .ToListAsync();

    public async Task<Specialty?> GetByIdAsync(int id)
        => await _context.Specialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Specialty> CreateAsync(Specialty specialty)
    {
        _context.Specialties.Add(specialty);
        await _context.SaveChangesAsync();
        return specialty;
    }

    public async Task UpdateAsync(Specialty specialty)
    {
        var existing = await _context.Specialties.FindAsync(specialty.Id);
        if (existing == null) return;

        existing.Name = specialty.Name;
        existing.Code = specialty.Code;
        existing.Faculty = specialty.Faculty;
        existing.BudgetPlaces = specialty.BudgetPlaces;
        existing.ContractPlaces = specialty.ContractPlaces;
        existing.Description = specialty.Description;
        existing.IsActive = specialty.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var s = await _context.Specialties.FindAsync(id);
        if (s != null)
        {
            _context.Specialties.Remove(s);
            await _context.SaveChangesAsync();
        }
    }
}

public class DocumentService
{
    private readonly AppDbContext _context;

    public DocumentService(AppDbContext context) => _context = context;

    public async Task<List<ApplicantDocument>> GetByApplicantAsync(int applicantId)
        => await _context.Documents
            .AsNoTracking()
            .Where(d => d.ApplicantId == applicantId)
            .ToListAsync();

    public async Task<ApplicantDocument> CreateAsync(ApplicantDocument doc)
    {
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();
        return doc;
    }

    public async Task UpdateAsync(ApplicantDocument doc)
    {
        var existing = await _context.Documents.FindAsync(doc.Id);
        if (existing == null) return;

        existing.IsProvided = doc.IsProvided;
        existing.IsVerified = doc.IsVerified;
        existing.FileName = doc.FileName;
        existing.Comment = doc.Comment;
        existing.UploadedAt = doc.UploadedAt;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var d = await _context.Documents.FindAsync(id);
        if (d != null)
        {
            _context.Documents.Remove(d);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> AreRequiredDocumentsVerifiedAsync(int applicantId)
    {
        var docs = await GetByApplicantAsync(applicantId);
        var required = new[]
        {
            AdmissionSystem.Enums.DocumentType.Passport,
            AdmissionSystem.Enums.DocumentType.EducationDocument,
            AdmissionSystem.Enums.DocumentType.NmtCertificate
        };
        return required.All(r => docs.Any(d => d.DocumentType == r && d.IsProvided && d.IsVerified));
    }
}