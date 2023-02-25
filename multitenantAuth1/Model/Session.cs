namespace multitenantAuth1.Model
{
    public class Session
    {
        public string? UpdatedBy { get; set; }
        public string? InsertedBy { get; set; }

        public string? tenantId { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public DateTime? InsertedTime { get; set; }

    }
}
