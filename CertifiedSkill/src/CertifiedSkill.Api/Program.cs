@page "/"
@inject CertifiedSkill.Application.Services.PersonService PersonService

<h3>CertifiedSkill</h3>

<button @onclick="CreateTestPerson">Create Person</button>

<p>@result</p>

@code {
    string? result;

private async Task CreateTestPerson()
{
    var person = await PersonService.CreatePersonAsync(
        "encrypted-demo",
        "hash-demo"
    );

    result = $"Created Person: {person.Id}";
}
}

app.MapPost("/certificates/issue", (CertificateService service) =>
{
    var cert = service.IssueCertificate(Guid.NewGuid(), "Clean Architecture 101");
    return Results.Ok(cert);
});