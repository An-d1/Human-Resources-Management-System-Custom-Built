using System;
using System.Threading.Tasks;

namespace HRManagementSystem.Models
{
    public interface ICompanyRepository
    {
        Task<Company> GetCompanyByIdAsync(Guid companyId);
        Task UpdateCompanyAsync(Company company);
        Task DeleteCompanyAsync(Company company);
        Task<string?> GetCompanyNameByCompanyIdAsync(Guid companyId);
        Task<Company> AddCompanyAsync(Company company);
        Task<List<Company>> GetAllCompaniesWhereIsActiveFalseAsync();
    }
}
