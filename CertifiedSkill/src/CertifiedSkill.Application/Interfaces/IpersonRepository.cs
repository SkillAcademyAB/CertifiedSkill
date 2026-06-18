using CertifiedSkill.Domain.Entities;

namespace CertifiedSkill.Application.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid id);
    Task<Person?> GetByNationalIdHashAsync(string hash);

    Task AddAsync(Person person);
    Task SaveChangesAsync();
}