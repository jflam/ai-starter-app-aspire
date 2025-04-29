using System;
using System.Collections.Generic;

namespace Data.Entities
{
    public class Sitter
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Bio { get; set; } = default!;
        public string HeroPhotoUrl { get; set; } = default!;
        public string Location { get; set; } = default!;
        public decimal PricePerNight { get; set; }
        public decimal StarRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime AvailabilityUpdatedAt { get; set; }
        public int RepeatClientCount { get; set; }

        public ICollection<SitterServiceType> SitterServiceTypes { get; set; } = new List<SitterServiceType>();
        public ICollection<SitterPetType> SitterPetTypes { get; set; } = new List<SitterPetType>();
        public ICollection<SitterBadge> SitterBadges { get; set; } = new List<SitterBadge>();
    }
}
