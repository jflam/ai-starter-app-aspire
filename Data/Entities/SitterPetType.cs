namespace Data.Entities
{
    public class SitterPetType
    {
        public int SitterId { get; set; }
        public Sitter Sitter { get; set; } = default!;
        public int PetTypeId { get; set; }
        public PetType PetType { get; set; } = default!;
    }
}
