using System;
using System.Collections.Generic;

namespace Client.Models
{
    public class SitterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HeroPhotoUrl { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public decimal StarRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime AvailabilityUpdatedAt { get; set; }
        public int RepeatClientCount { get; set; }
        public string Location { get; set; } = string.Empty;
        public List<string> ServiceTypes { get; set; } = new();
        public List<string> PetTypes { get; set; } = new();
        public List<string> Badges { get; set; } = new();
    }
}
