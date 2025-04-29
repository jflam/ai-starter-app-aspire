namespace Data.Entities
{
    public class SitterBadge
    {
        public int SitterId { get; set; }
        public Sitter Sitter { get; set; } = default!;
        public int BadgeId { get; set; }
        public Badge Badge { get; set; } = default!;
    }
}
