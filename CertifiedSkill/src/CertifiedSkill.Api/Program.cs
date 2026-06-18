using CertifiedSkill.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<CertificateService>();

var app = builder.Build();

app.MapGet("/", () => "CertifiedSkill API running");

app.MapPost("/certificates/issue", (CertificateService service) =>
{
    var cert = service.IssueCertificate(Guid.NewGuid(), "Clean Architecture 101");
    return Results.Ok(cert);
});

app.Run();