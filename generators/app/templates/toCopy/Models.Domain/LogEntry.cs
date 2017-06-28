namespace Models.Domain
{
    public class LogEntry : BaseModel
    {
        public string Level { get; set; }
        public int EventId { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public bool Exception { get; set; }
        public string IdentityUserId { get; set; }
        public string UserName { get; set; }
    }
}
