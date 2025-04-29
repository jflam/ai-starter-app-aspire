using System.Collections.Generic;

namespace Data.Entities
{
    public class PetType
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        public ICollection<SitterPetType> SitterPetTypes { get; set; } = new List<SitterPetType>();
    }
}
