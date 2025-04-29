# PetBnB Full Implementation Spec

This spec describes all low‑level changes to transform the original RandomFortuneApp into PetBnB with database schema, API, client UI, migrations, seeding, and AppHost wiring.
**Note**: Update *all* service references—DbContext registrations, connection string keys, appsettings values, AppHost wiring, and code references—from the original names (`FortuneDbContext`, `fortunesdb`) to the new PetBnB names (`PetBnBDbContext`, `petbnbdb`) consistently across the solution to avoid mismatches.

---

## 1. Data Layer

### 1.1 Rename DbContext
- Rename `Data/FortuneDbContext.cs` → `Data/PetBnBDbContext.cs`.
- Change class name `FortuneDbContext` → `PetBnBDbContext`.
- Change constructor signature to `DbContextOptions<PetBnBDbContext>`.
- Remove `DbSet<Fortune>`.
- Add:
  ```csharp
  public DbSet<Sitter> Sitters { get; set; }
  public DbSet<ServiceType> ServiceTypes { get; set; }
  public DbSet<PetType> PetTypes { get; set; }
  public DbSet<Badge> Badges { get; set; }
  public DbSet<SitterServiceType> SitterServiceTypes { get; set; }
  public DbSet<SitterPetType> SitterPetTypes { get; set; }
  public DbSet<SitterBadge> SitterBadges { get; set; }
  ```
- In `OnModelCreating`, configure composite keys for each join entity:
  ```csharp
  modelBuilder.Entity<SitterServiceType>().HasKey(e => new { e.SitterId, e.ServiceTypeId });
  modelBuilder.Entity<SitterPetType>().HasKey(e => new { e.SitterId, e.PetTypeId });
  modelBuilder.Entity<SitterBadge>().HasKey(e => new { e.SitterId, e.BadgeId });
  ```

### 1.2 Entity Models
Create `Data/Entities` folder with:
- **Sitter.cs**: properties `(Id, Name, Bio, HeroPhotoUrl, Location, PricePerNight, StarRating, ReviewCount, AvailabilityUpdatedAt, RepeatClientCount)` plus navigation collections for SitterServiceTypes, SitterPetTypes, SitterBadges.
- **ServiceType.cs**, **PetType.cs**, **Badge.cs**: each `(Id, Name)` plus navigation to relevant join collection.
- **SitterServiceType.cs**, **SitterPetType.cs**, **SitterBadge.cs**: each join entity with foreign keys, navigation properties.

### 1.3 Migrations
- Delete all existing migration files under `Data/Migrations` to remove legacy schema.
- Scaffold a fresh initial migration:
  ```pwsh
  dotnet ef migrations add CreatedDb -p Data -s AppHost
  dotnet ef database update -p Data -s AppHost
  ```

---

## 2. DbMigrations Project

### 2.1 Program.cs
- Change `builder.AddNpgsqlDbContext<FortuneDbContext>("fortunesdb")` → `builder.AddNpgsqlDbContext<PetBnBDbContext>("petbnbdb")`.
- Ensure every occurrence of the old connection key `fortunesdb` and type `FortuneDbContext` is updated to `petbnbdb` and `PetBnBDbContext` across all projects (programs, appsettings, migrations, AppHost, test scripts, etc.).
- Ensure `DatabaseSeeder` and `Worker` use `PetBnBDbContext`, injection updated.

### 2.2 Worker.cs
- Inject `PetBnBDbContext` instead of `FortuneDbContext`.
- After `dbContext.Database.MigrateAsync()`, resolve `DatabaseSeeder` and invoke `SeedDatabase()`.

### 2.3 DatabaseSeeder.cs
- Add `Bogus` NuGet dependency to `DbMigrations` project for realistic name generation.

- Seed reference data if empty:
  - ServiceTypes: `Boarding, House Sitting, Drop-In, Day Care, Walking`
  - PetTypes: `Dog, Cat`
  - Badges: `Verified Background Check, Star Sitter, Rover 101`
- Seed **25 random sitters** with:
  - Name: randomized first+last name.
  - HeroPhotoUrl: use DiceBear avatar API `https://api.dicebear.com/9.x/avataaars/svg?seed={Uri.EscapeDataString(name)}&eyes=default,eyeRoll,happy,hearts,surprised,wink,winkWacky&mouth=default,serious,smile,twinkle`.
  - Bio: pick a realistic bio from a curated list of 10 or more variations (e.g., "Lifelong animal lover...", "Certified pet CPR...", etc.).
  - Location: chosen from Puget Sound ZIP codes within 20 mi of Seattle.
  - PricePerNight: random 30–80.
  - StarRating: random 3.0–5.0 (1 decimal).
  - ReviewCount, RepeatClientCount: random.
  - AvailabilityUpdatedAt: DateTime.UtcNow minus 0–7 days.
- After saving sitters, assign random services, pets, badges to each via join tables.

---

## 3. Server API

### 3.1 Program.cs
- Register `PetBnBDbContext`:
  ```csharp
  builder.AddNpgsqlDbContext<PetBnBDbContext>("petbnbdb");
  ```
- In `appsettings.json`, add:
  ```json
  "ConnectionStrings": { "petbnbdb": "<YOUR_CONNECTION_STRING>" }
  ```

- Replace root fortune endpoint with **Search Endpoint**. **Important**: use array types (`string[]? serviceTypes`) for multi-value query params instead of `List<string>? serviceTypes` to avoid compilation errors.
  ```csharp
  app.MapGet("/api/v1/sitters/search", async (PetBnBDbContext db,
      string? location,
      decimal? minPrice,
      decimal? maxPrice,
      decimal? minRating,
      string[]? serviceTypes,
      string[]? petTypes,
      string[]? badges) =>
  {
      var query = db.Sitters
          .Include(s => s.SitterServiceTypes).ThenInclude(st => st.ServiceType)
          .Include(s => s.SitterPetTypes).ThenInclude(pt => pt.PetType)
          .Include(s => s.SitterBadges).ThenInclude(b => b.Badge)
          .AsQueryable();

      if (!string.IsNullOrWhiteSpace(location))
          query = query.Where(s => EF.Functions.ILike(s.Location, $"%{location}%"));
      if (minPrice.HasValue)
          query = query.Where(s => s.PricePerNight >= minPrice.Value);
      if (maxPrice.HasValue)
          query = query.Where(s => s.PricePerNight <= maxPrice.Value);
      if (minRating.HasValue)
          query = query.Where(s => s.StarRating >= minRating.Value);
      if (serviceTypes?.Any() == true)
          query = query.Where(s => s.SitterServiceTypes.Any(st => serviceTypes.Contains(st.ServiceType.Name)));
      if (petTypes?.Any() == true)
          query = query.Where(s => s.SitterPetTypes.Any(pt => petTypes.Contains(pt.PetType.Name)));
      if (badges?.Any() == true)
          query = query.Where(s => s.SitterBadges.Any(b => badges.Contains(b.Badge.Name)));

      var results = await query.Select(s => new {
          s.Id,
          s.Name,
          s.HeroPhotoUrl,
          s.PricePerNight,
          s.StarRating,
          s.ReviewCount,
          s.AvailabilityUpdatedAt,
          s.RepeatClientCount,
          s.Location,
          ServiceTypes = s.SitterServiceTypes.Select(x => x.ServiceType.Name),
          PetTypes = s.SitterPetTypes.Select(x => x.PetType.Name),
          Badges = s.SitterBadges.Select(x => x.Badge.Name)
      }).ToListAsync();

      return Results.Ok(results);
  });
  ```

---

## 4. Client App (Blazor/Razor Pages)

### 4.1 Models
- Create `Client/Models/SitterDto.cs` matching the API response shape.

### 4.2 API Client
- Rename `FortuneApiClient.cs` → `SitterApiClient.cs`.
- Implement `SearchSittersAsync(...)` building query string, calling `/api/v1/sitters/search` and parsing JSON to `List<SitterDto>`.

### 4.3 Program.cs (Client)
- Add `ApiBaseUrl` setting in `appsettings.json`.
- Register `SitterApiClient` via `AddHttpClient<SitterApiClient>` using `Configuration["ApiBaseUrl"]`.
- Ensure `MapRazorPages()` and static assets are mapped correctly.

### 4.4 Razor Page (`Index`)
- Replace fortune card with search UI:
  - Location input + auto-submit on change; remove manual Search button.
  - Reset button to clear filters.
  - Filters pane: Service Types, Pet Types, Badges; checkboxes auto-submit on change.
  - Remove "View Profile" buttons (no separate profile page yet).
  - Add pagination controls (8 per page) with Previous/Next links; bind `PageNumber` and `PageSize` in the page model.
- Listing grid:
  - Location input + Search/Reset buttons.
  - Filters pane: Service Types, Pet Types, Badges.
- Listing grid:
  - Card per sitter: photo, name, price, rating, reviews, repeat client count, location, services, pet types, badges, "X days ago" label, "View Profile" link.
- Empty state illustration + message + reset link.

- In `Index.cshtml.cs`:
  - Inject `SitterApiClient`.
  - Bind GET properties: Location, ServiceTypes, PetTypes, Badges.
  - OnGetAsync calls `sitterApiClient.SearchSittersAsync(...)` with bound filters.
  - Provide `ServiceTypesOptions`, `PetTypesOptions`, `BadgesOptions` for rendering filters.

---

## 5. AppHost Wiring

Update `AppHost/Program.cs`:
- Rename database node from `fortunesdb` → `petbnbdb`.
- `WaitFor`/`WithReference` for:
  - `Projects.Server` → depends on `petbnbdb`.
  - `Projects.DbMigrations` → depends on `petbnbdb`.
  - `Projects.Client` → depends on `Projects.Server`.

---

## 6. Build & Run

1. Scaffold and apply migrations:
   ```pwsh
   dotnet ef migrations add InitialPetBnBSchema -p Data -s AppHost
   dotnet ef database update -p Data -s AppHost
   ```
2. Seed and start:
   ```pwsh
   dotnet run --project DbMigrations --no-build
   dotnet run --project Server      --no-build
   dotnet run --project Client      --no-build
   ```
3. Open browser to `https://localhost:5001` and verify search UI & API.
