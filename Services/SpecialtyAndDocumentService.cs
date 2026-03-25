using AdmissionSystem.Data;
using AdmissionSystem.Enums;
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

    public DocumentService(AppDbContext context)
    {
        _context = context;
    }

    public List<ApplicantDocument> GetByApplicantId(int applicantId)
    {
        return _context.Documents
            .Where(d => d.ApplicantId == applicantId)
            .OrderBy(d => d.DocumentType)
            .ToList();
    }

    public async Task<List<ApplicantDocument>> GetByApplicantAsync(int applicantId)
    {
        return await _context.Documents
            .Where(d => d.ApplicantId == applicantId)
            .OrderBy(d => d.DocumentType)
            .ToListAsync();
    }

    public ApplicantDocument? GetById(int id)
    {
        return _context.Documents.FirstOrDefault(d => d.Id == id);
    }

    public async Task<ApplicantDocument?> GetByIdAsync(int id)
    {
        return await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
    }

    public void Create(ApplicantDocument document)
    {
        if (document.IsProvided && document.UploadedAt == null)
            document.UploadedAt = DateTime.Now;

        _context.Documents.Add(document);
        _context.SaveChanges();
    }

    public async Task CreateAsync(ApplicantDocument document)
    {
        if (document.IsProvided && document.UploadedAt == null)
            document.UploadedAt = DateTime.Now;

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
    }

    public void Update(ApplicantDocument document)
    {
        var existing = _context.Documents.FirstOrDefault(d => d.Id == document.Id);
        if (existing == null)
            return;

        ApplyDocumentChanges(existing, document);
        _context.SaveChanges();
    }

    public async Task UpdateAsync(ApplicantDocument document)
    {
        var existing = await _context.Documents.FirstOrDefaultAsync(d => d.Id == document.Id);
        if (existing == null)
            return;

        ApplyDocumentChanges(existing, document);
        await _context.SaveChangesAsync();
    }
    private static void ApplyDocumentChanges(ApplicantDocument existing, ApplicantDocument source)
    {
        existing.DocumentType = source.DocumentType;
        existing.FileName = source.FileName;
        existing.Comment = source.Comment;
        existing.IsProvided = source.IsProvided;
        existing.IsVerified = source.IsVerified;
        existing.UploadedAt = source.UploadedAt;

        ValidateDocument(existing);
    }
    public void Delete(int id)
    {
        var document = _context.Documents.FirstOrDefault(d => d.Id == id);
        if (document == null) return;

        _context.Documents.Remove(document);
        _context.SaveChanges();
    }

    public async Task DeleteAsync(int id)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null) return;

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
    }

    public void MarkAsProvided(int documentId, string? fileName = null, string? comment = null)
    {
        var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null) return;

        document.IsProvided = true;
        document.UploadedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(fileName))
            document.FileName = fileName;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        _context.SaveChanges();
    }

    public async Task MarkAsProvidedAsync(int documentId, string? fileName = null, string? comment = null)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null) return;

        document.IsProvided = true;
        document.UploadedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(fileName))
            document.FileName = fileName;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        await _context.SaveChangesAsync();
    }

    public void Verify(int documentId, string? comment = null)
    {
        var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null) return;

        if (!document.IsProvided)
            throw new InvalidOperationException("Не можна підтвердити документ, який не подано.");

        document.IsVerified = true;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        _context.SaveChanges();
    }

    public async Task VerifyAsync(int documentId, string? comment = null)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null) return;

        if (!document.IsProvided)
            throw new InvalidOperationException("Не можна підтвердити документ, який не подано.");

        document.IsVerified = true;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        await _context.SaveChangesAsync();
    }

    public void Unverify(int documentId, string? comment = null)
    {
        var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null) return;

        document.IsVerified = false;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        _context.SaveChanges();
    }

    public async Task UnverifyAsync(int documentId, string? comment = null)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null) return;

        document.IsVerified = false;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        await _context.SaveChangesAsync();
    }

    public void MarkAsMissing(int documentId, string? comment = null)
    {
        var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null) return;

        document.IsProvided = false;
        document.IsVerified = false;
        document.UploadedAt = null;
        document.FileName = string.Empty;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        _context.SaveChanges();
    }

    public async Task MarkAsMissingAsync(int documentId, string? comment = null)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null) return;

        document.IsProvided = false;
        document.IsVerified = false;
        document.UploadedAt = null;
        document.FileName = string.Empty;

        if (!string.IsNullOrWhiteSpace(comment))
            document.Comment = comment;

        await _context.SaveChangesAsync();
    }

    public void CreateDefaultDocumentsForApplicant(int applicantId)
    {
        var existingTypes = _context.Documents
            .Where(d => d.ApplicantId == applicantId)
            .Select(d => d.DocumentType)
            .ToHashSet();

        var requiredTypes = GetRequiredDocumentTypes();

        foreach (var type in requiredTypes)
        {
            if (!existingTypes.Contains(type))
            {
                _context.Documents.Add(new ApplicantDocument
                {
                    ApplicantId = applicantId,
                    DocumentType = type,
                    FileName = string.Empty,
                    IsProvided = false,
                    IsVerified = false,
                    UploadedAt = null,
                    Comment = string.Empty
                });
            }
        }

        _context.SaveChanges();
    }

    public async Task CreateDefaultDocumentsForApplicantAsync(int applicantId)
    {
        var existingTypes = await _context.Documents
            .Where(d => d.ApplicantId == applicantId)
            .Select(d => d.DocumentType)
            .ToListAsync();

        var requiredTypes = GetRequiredDocumentTypes();

        foreach (var type in requiredTypes)
        {
            if (!existingTypes.Contains(type))
            {
                _context.Documents.Add(new ApplicantDocument
                {
                    ApplicantId = applicantId,
                    DocumentType = type,
                    FileName = string.Empty,
                    IsProvided = false,
                    IsVerified = false,
                    UploadedAt = null,
                    Comment = string.Empty
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public bool HasAllRequiredDocuments(int applicantId)
    {
        var docs = _context.Documents
            .Where(d => d.ApplicantId == applicantId)
            .ToList();

        var requiredTypes = GetRequiredDocumentTypes();

        return requiredTypes.All(type =>
            docs.Any(d => d.DocumentType == type && d.IsProvided));
    }

    public bool HasAllVerifiedDocuments(int applicantId)
    {
        var docs = _context.Documents
            .Where(d => d.ApplicantId == applicantId)
            .ToList();

        var requiredTypes = GetRequiredDocumentTypes();

        return requiredTypes.All(type =>
            docs.Any(d => d.DocumentType == type && d.IsProvided && d.IsVerified));
    }

    private static List<DocumentType> GetRequiredDocumentTypes()
    {
        return new List<DocumentType>
        {
            DocumentType.Passport,
            DocumentType.EducationDocument,
            DocumentType.NmtCertificate
        };
    }

    private static void ValidateDocument(ApplicantDocument document)
    {
        if (document.IsVerified && !document.IsProvided)
            throw new InvalidOperationException("Не можна підтвердити документ, який не подано.");

        if (!document.IsProvided)
        {
            document.IsVerified = false;
            document.UploadedAt = null;
            document.FileName ??= string.Empty;
        }
    }
}