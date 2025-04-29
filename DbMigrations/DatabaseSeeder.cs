using Data;
using Data.Entities;
using Bogus;

namespace DbMigrations;

public class DatabaseSeeder
{
    private readonly PetBnBDbContext dbContext;
    public DatabaseSeeder(PetBnBDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task SeedDatabase()
    {
        // Seed ServiceTypes
        if (!dbContext.ServiceTypes.Any())
        {
            var services = new[] { "Boarding", "House Sitting", "Drop-In", "Day Care", "Walking" }
                .Select(n => new ServiceType { Name = n });
            await dbContext.ServiceTypes.AddRangeAsync(services);
        }
        // Seed PetTypes
        if (!dbContext.PetTypes.Any())
        {
            var pets = new[] { "Dog", "Cat" }
                .Select(n => new PetType { Name = n });
            await dbContext.PetTypes.AddRangeAsync(pets);
        }
        // Seed Badges
        if (!dbContext.Badges.Any())
        {
            var badges = new[] { "Verified Background Check", "Star Sitter", "Rover 101" }
                .Select(n => new Badge { Name = n });
            await dbContext.Badges.AddRangeAsync(badges);
        }
        await dbContext.SaveChangesAsync();

        // Seed Sitters
        if (!dbContext.Sitters.Any())
        {
            var faker = new Faker();
            var rng = new Random();
            var zipCodes = new[] { "98101", "98102", "98103", "98104", "98105", "98109", "98112", "98115", "98117", "98118", "98119", "98121", "98122", "98125", "98004", "98005", "98027", "98033", "98052", "98056" };
            var sitters = new List<Sitter>();
            for (int i = 0; i < 25; i++)
            {
                var name = faker.Name.FullName();
                // Generate a realistic bio
                var bios = new[] {
                    "Lifelong animal lover with 5 years of professional pet sitting experience.",
                    "Dedicated pet caretaker who treats every pet like family.",
                    "Certified pet CPR and first aid trained, ensuring safe and fun stays.",
                    "Passionate about outdoor adventures and daily walks with your furry friends.",
                    "Reliable and responsible sitter, available for last-minute bookings."
                };
                var bio = bios[rng.Next(bios.Length)];
                // Use DiceBear avatar API for sitter photos
                var photoUrl = $"https://api.dicebear.com/9.x/avataaars/svg?seed={Uri.EscapeDataString(name)}&eyes=default,eyeRoll,happy,hearts,surprised,wink,winkWacky&mouth=default,serious,smile,twinkle";
                sitters.Add(new Sitter
                {
                    Name = name,
                    HeroPhotoUrl = photoUrl,
                    Location = zipCodes[rng.Next(zipCodes.Length)],
                    PricePerNight = rng.Next(30, 81),
                    StarRating = Math.Round((decimal)(rng.NextDouble() * 2 + 3), 1),
                    ReviewCount = rng.Next(5, 201),
                    AvailabilityUpdatedAt = DateTime.UtcNow.AddDays(-rng.Next(0, 7)),
                    RepeatClientCount = rng.Next(0, 51),
                    Bio = bio
                });
            }
            await dbContext.Sitters.AddRangeAsync(sitters);
            await dbContext.SaveChangesAsync();

            // Assign random services, pets, and badges to each sitter
            var allServices = dbContext.ServiceTypes.ToList();
            var allPets = dbContext.PetTypes.ToList();
            var allBadges = dbContext.Badges.ToList();
            var serviceEntries = new List<SitterServiceType>();
            var petEntries = new List<SitterPetType>();
            var badgeEntries = new List<SitterBadge>();
            foreach (var sitter in sitters)
            {
                var serviceCount = rng.Next(1, allServices.Count + 1);
                foreach (var st in allServices.OrderBy(_ => rng.Next()).Take(serviceCount))
                    serviceEntries.Add(new SitterServiceType { SitterId = sitter.Id, ServiceTypeId = st.Id });
                var petCount = rng.Next(1, allPets.Count + 1);
                foreach (var pt in allPets.OrderBy(_ => rng.Next()).Take(petCount))
                    petEntries.Add(new SitterPetType { SitterId = sitter.Id, PetTypeId = pt.Id });
                var badgeCount = rng.Next(1, allBadges.Count + 1);
                foreach (var b in allBadges.OrderBy(_ => rng.Next()).Take(badgeCount))
                    badgeEntries.Add(new SitterBadge { SitterId = sitter.Id, BadgeId = b.Id });
            }
            await dbContext.SitterServiceTypes.AddRangeAsync(serviceEntries);
            await dbContext.SitterPetTypes.AddRangeAsync(petEntries);
            await dbContext.SitterBadges.AddRangeAsync(badgeEntries);
            await dbContext.SaveChangesAsync();
        }
    }
}
