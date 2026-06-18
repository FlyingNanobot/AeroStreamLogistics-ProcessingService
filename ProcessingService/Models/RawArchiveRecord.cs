namespace ProcessingService.Models
{
    public class RawArchiveRecord
    {
        public string Key { get; set; }
        public string RawJson { get; set; }
        public DateTime StoredAt { get; set; }
    }
}
