using CertifiedSkill.Application.Interfaces;
using CertifiedSkill.Domain.Entities;
using CertifiedSkill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;

    public PersonRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Person?> GetByIdAsync(Guid id)
        => _db.Persons.FirstOrDefaultAsync(x => x.Id == id);

    public Task<Person?> GetByNationalIdHashAsync(string hash)
        => _db.Persons.FirstOrDefaultAsync(x => x.NationalIdHash == hash);

    public async Task AddAsync(Person person)
        => await _db.Persons.AddAsync(person);

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();
}