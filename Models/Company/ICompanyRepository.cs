using System;
using System.Threading.Tasks;

namespace HRManagementSystem.Models
{
    public interface ICompanyRepository
    {
        Task<string?> GetCompanyNameByCompanyIdAsync(Guid companyId);
        Task<Company> AddCompanyAsync(Company company);
    }
}
