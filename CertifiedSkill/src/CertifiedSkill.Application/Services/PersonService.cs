using CertifiedSkill.Application.Interfaces;
using CertifiedSkill.Domain.Entities;

namespace CertifiedSkill.Application.Services;

public class PersonService
{
    private readonly IPersonRepository _repo;

    public PersonService(IPersonRepository repo)
    {
        _repo = repo;
    }

    public async Task<Person> CreatePersonAsync(string encryptedId, string hash)
    {
        var existing = await _repo.GetByNationalIdHashAsync(hash);
        if (existing != null)
            return existing;

        var person = new Person
        {
            Id = Guid.NewGuid(),
            EncryptedNationalId = encryptedId,
            NationalIdHash = hash
        };

        await _repo.AddAsync(person);
        await _repo.SaveChangesAsync();

        return person;
    }
}