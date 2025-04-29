namespace Data.Entities
{
    public class SitterServiceType
    {
        public int SitterId { get; set; }
        public Sitter Sitter { get; set; } = default!;
        public int ServiceTypeId { get; set; }
        public ServiceType ServiceType { get; set; } = default!;
    }
}
