namespace WebAPISample.Models
{
    public record AuditLogModel
    {
        public int UserId { get; init; }
        public string Message { get; init; }
    }
}