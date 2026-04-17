# CertifiedSkill

Certifierade kunskaper med fokus på spårbar identitet, kursresultat och verifierbara certifikat.

## Syfte

CertifiedSkill ska hantera personer, deltagare, kursresultat och certifikat på ett sätt som fungerar som system of record för utbildningsrelaterade bevis. Lösningen ska från start stödja säker hantering av persondata och vara förberedd för framtida extern identitet, särskilt BankID.

## Rekommenderad tech stack

Projektet bör fortsätta på nuvarande .NET-spår.

- **Backend/UI:** ASP.NET Core + Blazor Web App
- **Autentisering:** ASP.NET Core Identity för interna användare, kompletterat med egen deltagaridentitet för magic link och framtida BankID
- **Data:** Entity Framework Core + SQL Server
- **Känslig data:** kryptering i applikationslagret för PNR samt separat hash för dedupe och sök
- **Secrets och nycklar:** Azure Key Vault
- **Tester:** xUnit och integrationstester mot databas
- **Deployment senare:** GitHub Actions + Azure App Service + Azure SQL

### Varför inte Python först

- Nuvarande kodbas är redan byggd på ASP.NET Core, Blazor, Identity och EF Core
- Ett byte till Python nu skulle öka startkostnaden för Epic #1
- Python kan fortfarande användas senare för avgränsade batch- eller integrationsflöden, men inte som primär plattform

## Domänbegrepp

### Person

Den juridiska och verksamhetsmässiga huvudidentiteten. Person ska vara system of record och kopplas till:

- krypterat personnummer
- hashat personnummer för dedupe och sök
- samtycken
- externa identiteter
- deltagande, resultat och certifikat

### ExternalIdentity

Provider-neutral modell för extern identitet, till exempel BankID i framtiden. Den ska kunna länkas till exakt en Person och bära metadata om provider, status, länkning och revisionsspår.

### Consent

Spårbart samtycke kopplat till Person. Samtycket ska kunna versioneras, registreras, återkallas och auditeras.

## Säkerhetsprinciper för PNR

- PNR får aldrig lagras eller exponeras i klartext utanför strikt kontrollerade flöden
- PNR ska normaliseras innan validering, kryptering och hashing
- Sökning och dedupe ska ske via hashad representation, inte via klartext
- Krypteringsnycklar och hash-relaterade hemligheter ska hanteras separat från applikationskonfiguration
- Loggar, exportflöden och UI ska utformas så att PNR inte läcker

Se även `docs/security/pnr-key-management.md` för miljöregler, rotation, fallback och spårbarhet kring PNR-nycklar.

## Nästa steg

- Påbörja implementation i rekommenderad ordning
- Etablera teststrategi för PNR-validering, kryptering, hashning och identitetslänkning
