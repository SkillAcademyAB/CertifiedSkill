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

## Rekommenderad startordning för Epic #1

1. #11 Validering av personnummer
2. #10 Person entity med personnummer (krypterat)
3. #12 Hashad PNR för dedupe och sök
4. Ny story: Nyckelhantering för PNR-kryptering och hash
5. #13 Samtycke kopplat till person
6. #15 Magic link-inloggning för deltagare
7. #16 Begränsad deltagarvy (endast egna certifikat)
8. Ny story: Koppling mellan deltagaridentitet och person
9. #18 ExternalIdentity-modell (BankID provider)
10. #19 API för att länka extern identitet till person
11. Ny story: Integrationsgränssnitt och sandbox-strategi för BankID

## Backlogg för Epic #1

Epic #1: **Identity & Person (PNR + framtida BankID)**

### Feature #9: Person som System of Record

Status: behåll feature och komplettera med en ny story för nyckelhantering.

#### Story #11: Validering av personnummer (format + checksumma)

**Mål**
- Definiera accepterade format
- Definiera normalisering till internt standardformat
- Beskriva felregler och avvisade fall
- Säkerställa svenska testfall och edge cases

**Acceptance criteria**
- Systemet accepterar endast definierade svenska PNR-format
- Alla godkända format normaliseras till ett internt standardformat före vidare behandling
- Ogiltiga format och checksummefel avvisas med tydlig felorsak
- Dokumenterade edge cases finns för samordningsnummer, separatorer och längdvariationer om dessa ska stödjas eller nekas

**Tasks**
- Beskriv vilka inmatningsformat som stöds
- Beskriv normaliseringsregler före validering
- Beskriv checksummeregel och ogiltiga scenarier
- Dokumentera edge cases och avgränsningar
- Definiera testfall för giltiga och ogiltiga personnummer

#### Story #10: Person entity med personnummer (krypterat)

**Mål**
- Definiera Person som kärnmodell
- Beskriva vilka fält som är känsliga
- Definiera lagringsregler för krypterat PNR
- Definiera livscykel för skapa, läsa och uppdatera person

**Acceptance criteria**
- Person har tydligt definierad kärnmodell och ansvar
- Känsliga fält är identifierade och dokumenterade
- PNR lagras endast krypterat i databasen
- Det finns definierade regler för vilka flöden som får läsa eller ändra persondata

**Tasks**
- Beskriv Person-modellens fält och ansvar
- Märk ut känsliga respektive icke-känsliga attribut
- Beskriv hur PNR krypteras före persistens
- Beskriv regler för create, read och update
- Dokumentera beroenden till samtycke, identitet och resultat

#### Story #12: Hashad PNR för dedupe och sök

**Mål**
- Definiera normaliserad input till hash
- Definiera hash-strategi för dedupe och sök
- Beskriva konfliktfall och regler för unika träffar

**Acceptance criteria**
- Hash skapas alltid från samma normaliserade representation
- Dedupe kan upptäcka dubbletter utan att lagra klartext-PNR
- Sökning sker via hashat värde eller säkert indexerat alternativ
- Konfliktfall och manuella uppföljningsfall är definierade

**Tasks**
- Definiera input till hashning
- Beskriv hash-strategi och säkerhetskrav
- Beskriv regler för dubblettdetektion
- Beskriv sökflöde utan exponering av PNR
- Dokumentera konfliktfall och operativ hantering

#### Story #13: Samtycke kopplat till person

**Mål**
- Definiera consent-modell
- Versionera samtyckestext och källa
- Beskriva registrering, återkallelse och auditkrav

**Acceptance criteria**
- Samtycke kan kopplas till exakt en Person
- Samtyckestext eller policyversion är spårbar
- Registrering och återkallelse har tidsstämplar och aktörsspår
- Historik bevaras för revision

**Tasks**
- Definiera datamodell för samtycke
- Beskriv versionshantering av samtyckestext
- Beskriv registrerings- och återkallelseflöde
- Beskriv auditkrav och historik
- Definiera vilka verksamhetsflöden som kräver samtycke

#### Ny story: Nyckelhantering för PNR-kryptering och hash

**Mål**
- Definiera ägarskap och lagring för nycklar och hemligheter
- Definiera rotation och miljöhantering
- Beskriva fallback vid saknad eller ogiltig nyckel

**Acceptance criteria**
- Krypteringsnycklar och hash-hemligheter hanteras utanför kodbasen
- Miljöspecifika regler för dev, test och prod är definierade
- Nyckelrotation kan genomföras utan okontrollerad dataförlust
- Felhantering är definierad för saknade eller ogiltiga nycklar

**Tasks**
- Definiera var nycklar och hemligheter lagras
- Beskriv åtkomstmodell per miljö
- Beskriv rotationsstrategi
- Beskriv fallback och incidenthantering
- Definiera krav på övervakning och spårbarhet

### Feature #14: Deltagarautentisering och åtkomst

Status: byt namn från **Autentisering deltagare** till **Deltagarautentisering och åtkomst** och komplettera med en ny story.

#### Story #15: Magic link-inloggning för deltagare

**Mål**
- Definiera identifierare för inloggning
- Definiera tokenlivslängd, engångsanvändning och rate limiting
- Beskriva utskick, leveranskanal och skydd mot missbruk

**Acceptance criteria**
- Magic link kan endast användas inom definierad giltighetstid
- Token blir ogiltig efter användning eller utgång
- Rate limiting och spärrar mot missbruk är definierade
- Lyckad och ogiltig inloggning har tydliga utfall

**Tasks**
- Definiera vilket identifierande värde som används för att begära länk
- Beskriv tokenregler och livslängd
- Beskriv utskickskanal och leveranskrav
- Beskriv spärrar, rate limiting och återförsök
- Dokumentera lyckade och misslyckade scenarier

#### Story #16: Begränsad deltagarvy (endast egna certifikat)

**Mål**
- Definiera exakt vilken data deltagaren får se
- Definiera åtkomstregler per personidentitet
- Beskriva tomma, ogiltiga eller felaktiga accessfall

**Acceptance criteria**
- Deltagare kan endast se sina egna certifikat
- Ingen annan persons certifikat eller persondata exponeras
- Åtkomst nekas eller begränsas tydligt vid felaktig koppling
- Minimal dataprincip tillämpas i deltagarvyn

**Tasks**
- Lista vilka datafält som ska visas
- Definiera åtkomstregler mellan identitet och person
- Beskriv fel- och tomtillstånd
- Beskriv sekretesskrav för UI och API
- Definiera verksamhetstest för att säkerställa isolerad åtkomst

#### Ny story: Koppling mellan deltagaridentitet och person

**Mål**
- Definiera hur en autentiserad deltagaridentitet länkas till exakt en Person
- Beskriva konfliktfall när flera kandidater matchar
- Beskriva support- eller adminflöde för manuell resolution

**Acceptance criteria**
- Varje deltagaridentitet kan länkas entydigt till en Person
- Konfliktfall identifieras och hanteras utan att fel person exponeras
- Manuell resolution är definierad och auditerbar
- Felkoppling leder inte till åtkomst till annan persons data

**Tasks**
- Definiera länkstrategi mellan identitet och person
- Beskriv regler för automatisk respektive manuell matchning
- Beskriv konflikt- och eskaleringsflöden
- Beskriv auditkrav för länkning och om-länkning
- Definiera acceptansfall för entydig och tvetydig matchning

### Feature #17: Förbered BankID (ingen UI)

Status: behåll feature men förtydliga att scopet gäller integrationsberedskap, inte färdig användarupplevelse.

#### Story #18: ExternalIdentity-modell (BankID provider)

**Mål**
- Definiera en provider-neutral modell för extern identitet
- Definiera länk till Person
- Definiera statusfält, metadata och spårbarhet

**Acceptance criteria**
- ExternalIdentity stödjer flera providers utan ommodellering
- Länk mellan extern identitet och Person är tydligt definierad
- Status och metadata räcker för felsökning och revision
- Unik koppling per provider kan upprätthållas

**Tasks**
- Beskriv modellens fält och ansvar
- Definiera relation till Person
- Definiera provider-specifik metadata som måste bevaras
- Beskriv statusmodell för länkning och livscykel
- Definiera unicitetsregler per provider

#### Story #19: API för att länka extern identitet till person

**Mål**
- Definiera kontrakt för länkning
- Definiera idempotens och konfliktregler
- Beskriva audit- och samtyckekrav

**Acceptance criteria**
- API-kontraktet är tydligt definierat för lyckade och misslyckade länkningsförsök
- Upprepade anrop kan hanteras idempotent där det är relevant
- Konflikter kan identifieras utan att skapa dubbla länkar
- Auditkrav och samtyckekrav är dokumenterade

**Tasks**
- Beskriv API-kontrakt och anropsmönster
- Definiera idempotensregler
- Beskriv konflikt- och felhantering
- Beskriv audit- och samtyckekrav
- Definiera integrationstestfall för säker länkning

#### Ny story: Integrationsgränssnitt och sandbox-strategi för BankID

**Mål**
- Definiera abstraherat provider-interface
- Definiera callback- och session state-hantering
- Definiera sandbox/testmiljö och secrets-hantering

**Acceptance criteria**
- BankID kan anslutas senare utan att kärnmodellen behöver göras om
- Provider-interface separerar domänlogik från leverantörsspecifika detaljer
- Callback- och session state-hantering är definierad
- Sandbox och secrets-hantering är beskrivna per miljö

**Tasks**
- Definiera provider-interface och ansvar
- Beskriv callback-flöde och session state
- Beskriv sandbox-strategi och testmiljöer
- Beskriv secrets- och certifikathantering
- Definiera krav som möjliggör framtida BankID-integration utan ommodellering

## Föreslagna GitHub-ändringar

Följande ändringar bör göras i issue-hierarkin:

- Behåll Epic #1
- Uppdatera Feature #14 till namnet **Deltagarautentisering och åtkomst**
- Lägg till ny story under Feature #9: **Nyckelhantering för PNR-kryptering och hash**
- Lägg till ny story under Feature #14: **Koppling mellan deltagaridentitet och person**
- Lägg till ny story under Feature #17: **Integrationsgränssnitt och sandbox-strategi för BankID**
- Lägg till acceptance criteria i samtliga stories under Epic #1
- Lägg till task-checklistor i samtliga stories under Epic #1
- Märk samtliga stories med tydligt område och prioritet

## Nästa steg

- Uppdatera GitHub-issues enligt backloggen ovan
- Påbörja implementation i rekommenderad ordning
- Etablera teststrategi för PNR-validering, kryptering, hashning och identitetslänkning
