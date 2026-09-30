# Geonorge.Forvaltning

.NET 8 Web API som er backend for **Geonorge Forvaltning** – en løsning der organisasjoner kan definere egne datasett ("forvaltningsobjekter") med et fritt sett egenskaper, og deretter forvalte geografiske data i disse datasettene. Frontend-klienten ligger i et eget repo: [Geonorge.Forvaltning.Client](https://github.com/kartverket/Geonorge.Forvaltning.Client) (se `catalog-info.yaml` for system-kobling i Backstage).

Denne APIen eier **skjemadefinisjonene** (metadata om datasett og egenskaper) og et knippe tjenester som klienten ikke kan gjøre selv direkte mot Supabase: administrasjon, autorisasjon på egenskapsnivå, sanntidsvarsling, søk mot eksterne registre og geografisk analyse. Selve datalesingen/skrivingen av objektene gjøres i stor grad direkte fra klienten mot Supabase (Postgres) med Row Level Security – se avsnittet **Arkitektur** under.

## Arkitektur i korte trekk

- **Database:** PostgreSQL (Supabase). Metadata om datasett og egenskaper ligger i faste tabeller (`ForvaltningsObjektMetadata`, `ForvaltningsObjektPropertiesMetadata`, `AccessByProperties`), forvaltet av EF Core-migrasjoner i dette repoet.
- **Dynamiske datatabeller:** Når et datasett opprettes via `AdminController`, genererer `ObjectService` en egen tabell (`t_{datasetId}`) med kolonner ut fra de definerte egenskapene. Disse tabellene forvaltes med rå SQL (Npgsql), ikke EF Core, siden skjemaet er dynamisk.
- **Autentisering:** Brukere logger inn via Supabase Auth i klienten. API-et validerer bearer-token og Apikey mot Supabase (`AuthService`) og slår opp brukerens organisasjonsnummer i `public.users`-tabellen. Autorisasjon er organisasjonsbasert (eier, bidragsytere/`Contributors`, lesere/`Viewers`) og kan i tillegg styres per egenskap via `AccessByProperties`.
- **Sanntid:** `MessageHub` (SignalR, endpoint `/hubs/message`) brukes til å kringkaste peker-posisjoner og objektendringer mellom flere samtidige brukere i klienten (samskriving på kartet).
- **Geografisk analyse:** `AnalysisController`/`AnalysisService` bygger en rute (via ekstern ruteplanlegger) fra et startobjekt til objekter i et annet, koblet datasett – brukes f.eks. til nærhets-/rekkevidde-analyser mellom datasett som statsforvalterne har tilgang til (styrt av `Analysis`-seksjonen i konfig).
- **Eksterne oppslag:** `OrganizationSearchController` (Brønnøysundregistrene), `PlaceSearchController` (Geonorge stedsnavn-API) – begge proxyet og responscachet 24t.
- **E-post:** `EmailConfiguration`/MailKit brukes til varsling (f.eks. ved deling/tilgangsendringer – se `ObjectService`).

## Prosjektstruktur

```
Controllers/     API-endepunkter (Admin, Object, Analysis, OrganizationSearch, PlaceSearch)
Services/        Forretningslogikk (ObjectService, AuthService, Analysis/, Message/ (SignalR hub))
HttpClients/     Typed HttpClients mot eksterne tjenester (PlaceSearch, OrganizationSearch, RouteSearch)
Models/Entity/   EF Core-entiteter for metadata (migrert)
Models/Api/      DTO-er for API-kontrakten
Migrations/      EF Core-migrasjoner for metadata-tabellene
Middleware/      Global exception handling
```

## Kjøre lokalt

### Forutsetninger
- .NET 8 SDK
- Tilgang til en PostgreSQL-database (typisk et Supabase-prosjekt) med `public.users`-tabellen som forventes av `AuthService`
- Et Supabase-prosjekt for autentisering (URL + anon key)

### Oppsett
1. Sett `ASPNETCORE_ENVIRONMENT=Local` (se `Properties/launchSettings.json`) – dette gjør at `appsettings.Development.json` lastes i tillegg til/istedenfor `appsettings.json`. Denne filen er ikke sjekket inn og må opprettes lokalt, eller bruk [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (prosjektet har allerede en `UserSecretsId`).
2. Fyll ut følgende nøkler (se `appsettings.json` for full struktur):

   | Nøkkel | Beskrivelse |
   |---|---|
   | `ConnectionStrings:ForvaltningApiDatabase` | Postgres-tilkobling |
   | `Supabase:Url` | URL til Supabase-prosjektet (for token-validering) |
   | `GeoID:*` | Klient-ID/secret og endepunkter for GeoID/BAAT-autorisasjon |
   | `Email:WebmasterEmail`, `Email:SmtpHost` | Utsending av varsel-e-post |
   | `PlaceSearch:ApiUrl` | Geonorge stedsnavn-API (default satt i `appsettings.json`) |
   | `OrganizationSearch:ApiUrl` | Brønnøysundregistrenes enhetsregister (default satt) |
   | `RouteSearch:ApiUrl`, `RouteSearch:ApiKey` | OpenRouteService (rute for analyse-funksjonen) |
   | `Analysis:Datasets` | Hvilke datasett-par som kan analyseres mot hverandre, og hvilke organisasjoner som har tilgang |

3. Kjør migrasjonene (kjøres også automatisk ved oppstart via `dataContext.Database.Migrate()` i `Program.cs`, men kan også kjøres manuelt):
   ```bash
   dotnet ef database update --project Geonorge.Forvaltning
   ```
4. Start APIet:
   ```bash
   dotnet run --project Geonorge.Forvaltning
   ```
   API-et svarer på `https://localhost:44390`, med Swagger UI på `/docs`.

### CORS
I `Program.cs` er det definert en `Development`-CORS-policy som slipper til alle origins som starter med `https://localhost`. Dette matcher klientens lokale dev-server (se klient-repoet, port `44389`).

## Autentisering mot API-et

Alle beskyttede endepunkter forventer to headere, satt av klienten etter innlogging mot Supabase:
- `Authorization: Bearer <supabase-access-token>`
- `Apikey: <supabase-anon-key>`

`AuthService` validerer token mot Supabase sitt `/auth/v1/user`-endepunkt og slår deretter opp organisasjonstilhørighet i databasen.

## Datamodell (kort)

- **ForvaltningsObjektMetadata** – ett datasett: navn, beskrivelse, eierorganisasjon, `Contributors`/`Viewers` (organisasjonsnumre), om det er åpne data, samt hvilken tabell (`TableName`) dataene ligger i.
- **ForvaltningsObjektPropertiesMetadata** – én egenskap/kolonne i datasettet: navn, datatype, kolonnenavn, rekkefølge, om den er skjult, tillatte verdier, og egen tilgangsstyring (`Contributors`/`Viewers`).
- **AccessByProperties** – finkornet tilgang: hvilke organisasjoner/bidragsytere som får se/redigere basert på verdien i en gitt egenskap (brukes bl.a. av statsforvalter-scenarioer).

> Merk: Row Level Security (RLS)-policyene som skal håndheve mye av dette tilgangsregimet direkte i Postgres, er foreløpig kommentert ut som TODO i entitetsklassene og må vurderes/aktiveres i databasen.

## Relatert repo

- **Klient:** React-applikasjonen som konsumerer dette API-et og leser/skriver data direkte mot Supabase – se `Geonorge.Forvaltning.Client` sin README for detaljer om frontend-arkitekturen og hvordan de to henger sammen.
