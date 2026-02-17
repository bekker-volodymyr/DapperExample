namespace DapperIntro.Entities
{
    public class VisitorPassport
    {
        public int Id { get; set; }
        public string PassportNumber { get; set; } = null!;
        public long VisitorId { get; set; }
    }
}