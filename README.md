# Ticketify
**Ticketify** — a sample/realistic .NET 9 Web API for ticket management (API + Domain/Core + Infrastructure + Tests) with CI/CD to Azure and Infrastructure-as-Code (Bicep).

## Project overview
Ticketify is an ASP.NET Core Web API (targeting .NET 9) using:
* Clean, layered structure: ```API, Core, Infrastructure```
* EF Core (SqlServer) for persistence
* Repository pattern
* Serilog for logging (console + file)
* Seed data for quick demo
* Unit tests (xUnit) and test helpers
* Bicep templates (in ```.github/infra/templates```) for provisioning Azure resources (App Service, SQL Server, App Service Plan, etc.)

This repo includes a ready GitHub Actions workflow to build/publish the API and an example Bicep for automating infra creation.

## Tech Stack
- **Language:** C#
- **Backend (API):** ASP.NET Core Web API (.NET 9), Entity Framework Core (Code-First + Migrations), Serilog for logging   
- **Database:** Azure SQL Database, EF Core Migrations
- **Infrastructure & Cloud:** Azure App Service, Azure SQL Database, Azure Resource Group, Bicep template for Infrastructure as Code (IaC), Service Principal for secure deployment automation
- **DevOps / CI-CD:** Github actions for build, test and deploy, automated build -> publish -> deploy pipelines, secrets management via Github Environments & azure app settings.
- **Testing:** xUnit for unit testing, In-Memery EF Core for repository testing. 
- **Architecture:** Clean architecture, Interface-driven design, dependency injection, EntityFramework Core, in-memory store (test)


## Project Structure
```text
Ticketify/
│
├── API/                                            # ASP.NET Core Web API project (entrypoint)
│   └── Controllers/
│   |   └── BaseApiController.cs
│   |   └── TicketsController.cs
│   └── Errors/
│   |   └── ApiErrorResponse.cs
│   └── logs/
│   └── Middleware
│   |   └── ExceptionMiddleware.cs
│   |   └── LogRequestMiddleware.cs
│   └── API.http
│   └── appsettings.json
│   └── appsettings.Development.json
│   └── Program.cs
│
├── Core/                                           # Domain entities, interfaces, request helpers (PagedList, TicketParams)
│   └── Entities/
│   |   └── AppUser.cs
│   |   └── BaseEntity.cs
│   |   └── Ticket.cs
│   └── Interfaces/
│   |   └── ITicketRepository.cs
│   └── RequestHelpers/
│   |   └── PagedList.cs
│   |   └── TicketParams.cs
|
├── Infrastructure/                                 # EF Core DbContext, Repositories, Migrations, Seed data
│   └── Data/
│   |   └── SeedData/
|   |   |   └── tickets.json
│   |   └── TicketContext.cs
│   |   └── TicketContextSeed.cs
│   └── Migrations/                                 # EF Migrations (committed)
│   └── Repositories/
│   |   └── TicketRepository.cs
|
├── Test/                                           # xUnit tests (Test.csproj)
│   └── Helpers/
│   |   └── TestDbContextFactory.cs
│   |   └── TicketDataHelper.cs
│   └── TicketControllerTests.cs                    # Controller tests
│   └── TicketRepositoryTests.cs                    # Repository tests
│
├─ .github/infra/templates/                         # Bicep (main_dev.bicep)
├─ .github/workflows/                               # CI/CD workflows (build & deploy)
├─ Ticketify.sln
└─ README.md
|
```
## Local setup & run
**Prerequisites**
* .NET SDK 9.x installed
* SQL Server locally / Azure SQL connection string for production testing

**1. Clone the repo**
```
git clone https://github.com/Suraj-Varade/Ticketify.git

cd Ticketify
```
- Use appsettings.Development.json for local development (do NOT commit secrets).
- Alternatively, you can aslo use dotnet user-secrets for local secrets
```text
"ConnectionStrings": {
    "DefaultConnection": "{yourDbConnectionString}"
  },
```

**2. Restore, build and run**
```text
# from repo root
dotnet restore Ticketify.sln

# build only the API project
dotnet build /API/API.csproj -c Release

# (Optional) If you want to run tests
dotnet test

# publish (local verification)
dotnet publish /API/API.csproj -c Release -o ./publish_output

# run the published app
cd publish_output

dotnet API.dll

# or from project
dotnet run --project /API/API.csproj -c Release
```

After the app starts, test:
http://0.0.0.0:8080/api/tickets

**Ef migrations:**
* dotnet-ef tool if you manage migrations locally:
```text
dotnet tool install --global dotnet-ef
```

## Entity Framework, Migrations & Seeding
**Key points**
* Migrations are committed under Infrastructure/Migrations/ (so CI/CD & runtime can apply them).
* On startup the app applies migrations only via context.Database.MigrateAsync() and then runs seeding (TicketContextSeed.SeedAsync(context)).

**Important**: Do **not** call EnsureCreated() in production when you plan to use migrations — EnsureCreated() and Migrate() conflict and can cause errors like "There is already an object named 'Tickets' in the database."

**Local migration workflow**
If you want to create/update migrations locally:
```text
# Add a migration (project containing DbContext is Infrastructure)
dotnet ef migrations add InitialCreate -p Infrastructure/Infrastructure.csproj -s API/API.csproj

# Apply locally (if connection string points to your local DB)
dotnet ef database update -p Infrastructure/Infrastructure.csproj -s API/API.csproj
```
**Running migrations during CI/CD / Deploy**
You can:
* Run dotnet ef database update as a step in your GitHub Action (recommended for dev/integration), or
* Keep context.Database.MigrateAsync() in startup (wrap in try-catch and log), but ensure migrations are committed and DB state is compatible (avoid EnsureCreated).

**Tests & Code Coverage**
* Tests are in Test/ (xUnit).
* **Important CI note**: Do not publish test artifacts to production deployment output. Publish only the API project output.
* Coverlet (code coverage) may be present; avoid instrumenting builds that produce the final publish artifact to be deployed. In CI, run tests in a separate step and don't copy bin/Debug test outputs to artifact folder.

Example test command (CI):
```
- name: Run tests
  run: dotnet test Ticketify.sln --configuration Release
```

Ensure tests are built before running tests (remove --no-build if you want to build in the same job).

## GitHub Actions CI/CD (recommended pipeline)
High-level flow:
1. Checkout
2. Setup .NET (DOTNET_VERSION: '9.0.x')
3. Restore dependencies (solution)
4. Build API project
5. Run tests (optional; ensure tests succeed before publish)
6. Clean publish folder
7. Publish API project to publish_output
8. Upload artifact
9. Azure login (service principal via AZURE_CREDENTIALS secret)
10. Optionally deploy infrastructure with Bicep (az deployment group create)
11. Deploy published artifact to Azure Web App (azure/webapps-deploy@v3)
12. Configure Linux App Service startup command and app settings (az webapp config set / appsettings set)
**Crucial**: publish the API project only, and ensure publish_output contains only runtime artifacts. 

## Infrastructure as Code — Bicep automation
You have a Bicep template in:
```text
.github/infra/templates/main_dev.bicep
```
The recommended automation:
1. Create a GitHub secret AZURE_CREDENTIALS containing your service principal JSON (clientId, clientSecret, subscriptionId, tenantId).
2. In the workflow, after azure/login@v2, run: 
```text
- name: Deploy Azure resources (Bicep)
  uses: azure/CLI@v2
  with:
    inlineScript: |
      az group create --name myResourceGroup --location eastus
      az deployment group create \
        --resource-group myResourceGroup \
        --template-file ./.github/infra/templates/main_dev.bicep \
        --parameters webAppName=${{ env.AZURE_WEBAPP_NAME }} \
                     sqlAdminPassword=${{ secrets.AZURE_SQL_PASSWORD }}

```
3. Capture outputs if needed. Use the created web app name for subsequent deployment.

## Azure App Service runtime notes & environment variables
* Linux App Service: Oryx will create the startup script and run the startup command you provide. Linux ignores web.config.
* **Important**: Your app must listen on the port specified by environment variable PORT. Add explicit binding:
```text
// in Program.cs before app.Run()
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
```
* **App settings you should add in App Service:**
* ConnectionStrings__DefaultConnection : <your-azure-sql-conn-string>
* ASPNETCORE_ENVIRONMENT : Development(if dev)/Production
* (Optional) ASPNETCORE_URLS : http://+:8080
* Application Insights connection string / instrumentation key.
* **Startup command (Linux)**: set to dotnet API.dll (or exact dll name if different).

## Troubleshooting / Common gotchas (diagnosed & fixed)
1. **Publishing test artifacts to /wwwroot**
* Cause: publishing whole solution or leaving build/test outputs in publish folder.
* Fix: publish API.csproj only and clean publish_output before publish.

2. **App not starting / 504 Gateway Timeout**
* Cause: app listening on wrong port (e.g., default 5000/8181).
* Fix: bind to PORT env var or set ASPNETCORE_URLS/Kestrel binding.

3. **Migrations crashing startup**
* Cause: EnsureCreated() used together with migrations or migrations re-applying already-created tables.
* Fix: Remove EnsureCreated() and use MigrateAsync() only. If DB already had tables created without migration history, either drop tables or insert migration entries in __EFMigrationsHistory.

4. **Coverlet / test-run error in CI**
* Cause: running tests with --no-build but test DLL missing, or coverlet cannot find module.
* Fix: build test projects before running tests, or run tests in a separate job, and do not copy test outputs to production publish folder.

5. **Startup failing in Azure due to unhandled migration exceptions**
* Fix: wrap migration/seed in try-catch and log exceptions so the app doesn't crash silently. Or run migrations in a controlled step before deployment.

## Useful commands (summary)
Build & publish API:
```
dotnet restore Ticketify.sln
dotnet build /API/API.csproj -c Release
dotnet publish /API/API.csproj -c Release -o ./publish_output
```

Add migration (local dev):
```
dotnet ef migrations add YourMigrationName -p Infrastructure/Infrastructure.csproj -s API/API.csproj
```

Apply migrations to DB:
```
# uses connection string from appsettings or environment
dotnet ef database update -p Infrastructure/Infrastructure.csproj -s API/API.csproj
```

Clean webroot from Kudu (if needed):
```
# run in Kudu console or Azure CLI via SSH
rm -rf /home/site/wwwroot/*
```

Bicep deployment via CLI:
```
az login --service-principal -u <clientId> -p <clientSecret> --tenant <tenantId>
az group create -n myRG -l eastus
az deployment group create -g myRG --template-file ./.github/infra/templates/main_dev.bicep \
--parameters webAppName=myticketify sqlAdminPassword=<secret>
```

## Where to look in the repo
* ```API/Program.cs``` — app setup, logging, migrations, port binding.
* ```Infrastructure/Migrations/``` — migration files. Commit these so Migrate() can apply them in Azure.
* ```.github/infra/templates/main_dev.bicep``` — bicep file used to create SQL server, app service plan, web app, etc.
* ```.github/workflows/``` — CI/CD definitions (edit to implement infra deploy step).

## Final recommended additions
* Add README.md (this file) to root.
* Add a small deploy-infra.yml workflow or extend existing CI to:
  * login with service principal, 
  * run the Bicep deployment, 
  * then deploy the published artifact.
