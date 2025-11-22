using HRManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem.Models;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context; 
    }

    public async Task<Company> GetCompanyByIdAsync(Guid companyId)
    {
        return await _context.Companies
        .FirstOrDefaultAsync(i => i.Id == companyId);
    }
    public async Task UpdateCompanyAsync(Company company)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteCompanyAsync(Company company)
    {
        _context.Companies.Remove(company);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<string?> GetCompanyNameByCompanyIdAsync(Guid companyId)
    {
        var companyName = await _context.Companies
            .Where(c => c.Id == companyId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();

        return companyName;
    }
    public async Task<Company> AddCompanyAsync(Company company)
    {
        // Check email uniqueness
        bool emailExists = await _context.Companies
            .AnyAsync(c => c.CompanyEmail == company.CompanyEmail);

        if (emailExists)
            throw new InvalidOperationException("A company with this email already exists.");

        // Check phone number uniqueness
        bool phoneExists = await _context.Companies
            .AnyAsync(c => c.CompanyPhoneNumber == company.CompanyPhoneNumber);

        if (phoneExists)
            throw new InvalidOperationException("A company with this phone number already exists.");

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task<List<Company>> GetAllCompaniesWhereApprovalStatusNullAsync()
    {
        return await _context.Companies
            .Where(c => c.ApprovalStatus == null) // gets all the companies with approvalStatus = null, which means pending application
            .ToListAsync();
    }

}
