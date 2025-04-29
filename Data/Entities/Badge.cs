using System.Collections.Generic;

namespace Data.Entities
{
    public class Badge
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        public ICollection<SitterBadge> SitterBadges { get; set; } = new List<SitterBadge>();
    }
}
