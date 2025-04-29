# PetBnB Full Implementation Spec (v2)

## Overview
This spec describes all low‑level changes to transform the original RandomFortuneApp into PetBnB, including:
- Database schema, migrations, seeding
- Server API endpoints and resource naming
- Client UI (Razor Pages) with auto‑submit filters, dynamic options, and pagination
- AppHost project wiring and service ordering

Key rules/takeaways:
1. Forms and filter controls auto‑submit on change; no manual "Search" button needed.
2. Maintain separate collections for `AvailableOptions` (to render) and `SelectedOptions` (bound to query).  
3. API must expose endpoints for dynamic filter lists plus a search endpoint with array query params.  
4. Resource naming:  
   - `/api/v1/sitters/search`  
   - `/api/v1/filters/service-types`  
   - `/api/v1/filters/pet-types`  
   - `/api/v1/filters/badges`  
5. Pagination via `pageNumber` and `pageSize` query params, defaulting to 1 and 8.
6. UI: render cards with sitter photo, stats, badges, location, and "X days ago" label; remove "View Profile" links.
7. Reset control clears all selected filters and resets paging.

---

## 1. Data Layer

### 1.1 Rename DbContext
- Move/rename `Data/FortuneDbContext.cs` → `Data/PetBnBDbContext.cs`.
- Class: `public class PetBnBDbContext : DbContext` with ctor accepting `DbContextOptions<PetBnBDbContext>`.
- Remove `DbSet<Fortune>`. Add:
  ```csharp
  public DbSet<Sitter> Sitters { get; set; }
  public DbSet<ServiceType> ServiceTypes { get; set; }
  public DbSet<PetType> PetTypes { get; set; }
  public DbSet<Badge> Badges { get; set; }
  public DbSet<SitterServiceType> SitterServiceTypes { get; set; }
  public DbSet<SitterPetType> SitterPetTypes { get; set; }
  public DbSet<SitterBadge> SitterBadges { get; set; }
  ```
- In `OnModelCreating()`, configure composite keys:
  ```csharp
  modelBuilder.Entity<SitterServiceType>().HasKey(e => new { e.SitterId, e.ServiceTypeId });
  modelBuilder.Entity<SitterPetType>().HasKey(e => new { e.SitterId, e.PetTypeId });
  modelBuilder.Entity<SitterBadge>().HasKey(e => new { e.SitterId, e.BadgeId });
  ```

### 1.2 Entity Models
- `Sitter.cs` properties:  
  `Id`, `Name`, `Bio`, `HeroPhotoUrl`, `Location`, `PricePerNight`, `StarRating`, `ReviewCount`,  
  `AvailabilityUpdatedAt`, `RepeatClientCount`, plus navigation to join collections.
- `ServiceType.cs`, `PetType.cs`, `Badge.cs`: each \(Id, Name\) + navigation.
- Join entities: `SitterServiceType`, `SitterPetType`, `SitterBadge`.

### 1.3 Migrations
- Delete legacy migrations under `Data/Migrations`.
- Scaffold fresh initial migration:
  ```pwsh
  dotnet ef migrations add CreatedDb -p Data -s AppHost
  dotnet ef database update -p Data -s AppHost
  ```

---

## 2. DbMigrations Project

### 2.1 Program.cs / Worker.cs
- Register `PetBnBDbContext` with Npgsql, key `petbnbdb`.
- Ensure `DatabaseSeeder` invoked after `dbContext.Database.MigrateAsync()`.

### 2.2 DatabaseSeeder.cs
- Add `Bogus` NuGet for random data.
- Seed reference tables if empty:
  - ServiceTypes: `Boarding`, `House Sitting`, `Drop-In`, `Day Care`, `Walking`.
  - PetTypes: `Dog`, `Cat`.
  - Badges: `Verified Background Check`, `Star Sitter`, `Rover 101`.
- Seed 25 random sitters:
  - Realistic first/last names via Bogus.
  - `HeroPhotoUrl` using DiceBear:  
    `https://api.dicebear.com/9.x/avataaars/svg?seed={Uri.EscapeDataString(name)}&eyes=…`
  - `Bio`: pick from a curated list of 10+ bios.
  - `Location`: random Puget Sound ZIP within 20 mi of Seattle.
  - `PricePerNight`: 30–80.
  - `StarRating`: 3.0–5.0, one decimal.
  - `ReviewCount`, `RepeatClientCount`: random integers.
  - `AvailabilityUpdatedAt`: `DateTime.UtcNow.AddDays(-rand0to7)`.
- After saving sitters, assign each random services, pet types, badges via join tables.

---

## 3. Server API

### 3.1 Program.cs
- Register `PetBnBDbContext`:
  ```csharp
  builder.AddNpgsqlDbContext<PetBnBDbContext>("petbnbdb");
  ```
  
### 3.2 Filter Option Endpoints
- GET `/api/v1/filters/service-types` → `List<string>` (all service names).
- GET `/api/v1/filters/pet-types` → `List<string>`.
- GET `/api/v1/filters/badges` → `List<string>`.

### 3.3 Search Endpoint
- GET `/api/v1/sitters/search`
- Query params:
  - `string? location` (partial-match, ILike)
  - `decimal? minPrice`, `decimal? maxPrice`
  - `decimal? minRating`
  - `string[]? serviceTypes`, `string[]? petTypes`, `string[]? badges`
  - `int pageNumber = 1`, `int pageSize = 8`
- Build EF query with `Include(s => ...)` and conditional filters.
- Use `Skip((pageNumber-1)*pageSize).Take(pageSize)` for pagination.
- Return:
  ```json
  [
    {
      "id": 1,
      "name": "Alice",
      "heroPhotoUrl": "...",
      "pricePerNight": 50,
      "starRating": 4.5,
      "reviewCount": 12,
      "availabilityUpdatedAt": "2025-04-29T...Z",
      "repeatClientCount": 3,
      "location": "98101",
      "serviceTypes": ["Boarding","Walking"],
      "petTypes": ["Dog"],
      "badges": ["Star Sitter"]
    }
  ]
  ```

---

## 4. Client App (Blazor / Razor Pages)

### 4.1 Models
- Create `Client/Models/SitterDto.cs` matching above JSON shape.

### 4.2 API Client
- Rename `FortuneApiClient.cs` → `SitterApiClient.cs`.
- Implement methods:
  ```csharp
  Task<List<string>> GetServiceTypesAsync();
  Task<List<string>> GetPetTypesAsync();
  Task<List<string>> GetBadgesAsync();
  Task<List<SitterDto>> SearchSittersAsync(string? location, decimal? minPrice, decimal? maxPrice, decimal? minRating,
      List<string>? serviceTypes, List<string>? petTypes, List<string>? badges,
      int pageNumber, int pageSize);
  ```
- Build query string with array params e.g. `?serviceTypes=Boarding&serviceTypes=Walking`.

### 4.3 Program.cs (Client)
- Add `ApiBaseUrl` in `appsettings.json`.
- Register:
  ```csharp
    builder.Services.AddHttpClient<SitterApiClient>(client =>
    {
        client.BaseAddress = new("https+http://server");
    });
  ```
- Ensure `builder.Services.AddRazorPages()` and `app.MapRazorPages()`.

### 4.4 Index Razor Page (UI)
- `@page "/"` with no form tag submit. All inputs auto‑submit on change.
- Inject `SitterApiClient`.
- PageModel properties:
  ```csharp
  [BindProperty(SupportsGet=true)] public int PageNumber { get; set; } = 1;
  [BindProperty(SupportsGet=true)] public int PageSize { get; set; } = 8;
  [BindProperty(SupportsGet=true)] public string? Location { get; set; }
  [BindProperty(SupportsGet=true)] public List<string>? SelectedServiceTypes { get; set; }
  [BindProperty(SupportsGet=true)] public List<string>? SelectedPetTypes { get; set; }
  [BindProperty(SupportsGet=true)] public List<string>? SelectedBadges { get; set; }

  public List<string> AvailableServiceTypes { get; set; }
  public List<string> AvailablePetTypes { get; set; }
  public List<string> AvailableBadges { get; set; }
  public List<SitterDto> Sitters { get; set; }
  ```
- In `OnGetAsync()`:  
  1. Load `Available*` via filter endpoints.  
  2. Call `SearchSittersAsync(...)` with selected filters and paging.  

- UI markup:
  - Text input for `Location` with `@bind` and `onchange` triggers page reload.
  - For each filter category, render checkboxes from `Available*`; each checkbox bound to `Selected*` collection; `onchange` triggers GET with current query.
  - Render "Reset" link clearing all selected values and resetting `PageNumber` to 1.
  - Render grid of sitter cards (8 per page): photo, name, price, star rating, review count, repeat client count, badges list, location, "X days ago" since `AvailabilityUpdatedAt`.
  - Pagination footer with "Previous" and "Next" links, disabling as needed; page links include current query params.
  - No separate "View Profile" link.
  - Empty state: display illustration + "No sitters found" + "Reset filters" link.

---

## 5. AppHost Wiring

In `AppHost/Program.cs`:
- Rename database node from `fortunesdb` → `petbnbdb`.
- Start order:
  1. petbnbdb  
  2. DbMigrations → references petbnbdb  
  3. Server → references petbnbdb  
  4. Client → references Server  

Example:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var petbnbdb = builder.AddPostgres("postgresql").AddDatabase("petbnbdb");

var server = builder.AddProject<Projects.Server>("server")
                    .WaitFor(petbnbdb)
                    .WithReference(petbnbdb);

builder.AddProject<Projects.DbMigrations>("dbmigrations")
       .WaitFor(petbnbdb)
       .WithReference(petbnbdb);

builder.AddProject<Projects.Client>("client")
       .WaitFor(server)
       .WithReference(server);

builder.Build().Run();
```

---

## 6. Build & Run

1. Scaffold & apply migrations:
   ```pwsh
   dotnet ef migrations add CreatedDb -p Data -s AppHost
   dotnet ef database update -p Data -s AppHost
   ```
2. Run migrations/seeder, server, client:
   ```pwsh
   dotnet run --project DbMigrations --no-build
   dotnet run --project Server      --no-build
   dotnet run --project Client      --no-build
   ```
3. Visit `https://localhost:5001` to verify:
   - Filters auto‑submit and update results
   - Pagination works
   - Dynamic filter options load correctly

---

_End of spec v2._
