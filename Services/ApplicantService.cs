using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdmissionSystem.Data;
using AdmissionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Services;

public class ApplicantService
{
    private readonly AppDbContext _context;

    public ApplicantService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Applicant>> GetAllAsync()
    {
        return await _context.Applicants
            .AsNoTracking()
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .ToListAsync();
    }

    public async Task<Applicant?> GetByIdAsync(int id)
    {
        return await _context.Applicants
            .AsNoTracking()
            .Include(a => a.Applications)
                .ThenInclude(app => app.Specialty)
            .Include(a => a.Applications)
                .ThenInclude(app => app.StatusHistory)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Applicant>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = searchTerm.ToLower();
        return await _context.Applicants
            .AsNoTracking()
            .Where(a =>
                a.LastName.ToLower().Contains(term) ||
                a.FirstName.ToLower().Contains(term) ||
                a.MiddleName.ToLower().Contains(term) ||
                a.Email.ToLower().Contains(term) ||
                a.Phone.Contains(term) ||
                a.DocumentSeriesNumber.ToLower().Contains(term))
            .OrderBy(a => a.LastName)
            .ToListAsync();
    }

    public async Task<Applicant> CreateAsync(Applicant applicant)
    {
        applicant.RegistrationDate = DateTime.Now;
        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync();
        return applicant;
    }

    public async Task UpdateAsync(Applicant applicant)
    {
        // Знаходимо існуючий запис і оновлюємо поля вручну
        var existing = await _context.Applicants.FindAsync(applicant.Id);
        if (existing == null) return;

        existing.LastName = applicant.LastName;
        existing.FirstName = applicant.FirstName;
        existing.MiddleName = applicant.MiddleName;
        existing.DateOfBirth = applicant.DateOfBirth;
        existing.Gender = applicant.Gender;
        existing.Phone = applicant.Phone;
        existing.Email = applicant.Email;
        existing.Address = applicant.Address;
        existing.DocumentSeriesNumber = applicant.DocumentSeriesNumber;
        existing.TaxCode = applicant.TaxCode;
        existing.AverageGrade = applicant.AverageGrade;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var applicant = await _context.Applicants.FindAsync(id);
        if (applicant != null)
        {
            _context.Applicants.Remove(applicant);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Applicants.AnyAsync(a => a.Id == id);
}