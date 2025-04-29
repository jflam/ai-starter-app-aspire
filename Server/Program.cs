using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PetBnBDbContext>("petbnbdb");
var app = builder.Build();

app.MapDefaultEndpoints();

// Sitters search API endpoint with filters
app.MapGet("/api/v1/sitters/search", async (PetBnBDbContext db,
    string? location,
    decimal? minPrice,
    decimal? maxPrice,
    decimal? minRating,
    string[]? serviceTypes,
    string[]? petTypes,
    string[]? badges,
    int pageNumber = 1,
    int pageSize = 8) =>
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
    if (serviceTypes != null && serviceTypes.Any())
        query = query.Where(s => s.SitterServiceTypes.Any(st => serviceTypes.Contains(st.ServiceType.Name)));
    if (petTypes != null && petTypes.Any())
        query = query.Where(s => s.SitterPetTypes.Any(pt => petTypes.Contains(pt.PetType.Name)));
    if (badges != null && badges.Any())
        query = query.Where(s => s.SitterBadges.Any(b => badges.Contains(b.Badge.Name)));

    var results = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(s => new {
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
        })
        .ToListAsync();
    return Results.Ok(results);
 });

 // Filter options endpoints
 app.MapGet("/api/v1/filters/service-types", async (PetBnBDbContext db) =>
     Results.Ok(await db.ServiceTypes.Select(st => st.Name).ToListAsync()));
 app.MapGet("/api/v1/filters/pet-types", async (PetBnBDbContext db) =>
     Results.Ok(await db.PetTypes.Select(pt => pt.Name).ToListAsync()));
 app.MapGet("/api/v1/filters/badges", async (PetBnBDbContext db) =>
     Results.Ok(await db.Badges.Select(b => b.Name).ToListAsync()));

 app.Run();