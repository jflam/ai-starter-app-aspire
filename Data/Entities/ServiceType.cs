using System.Collections.Generic;

namespace Data.Entities
{
    public class ServiceType
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        public ICollection<SitterServiceType> SitterServiceTypes { get; set; } = new List<SitterServiceType>();
    }
}
