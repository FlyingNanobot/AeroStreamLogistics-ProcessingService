namespace ProcessingService.Models
{
    public class KafkaEnvelope<T>
    {
        public string MessageId { get; set; }
        public DateTime ReceivedAt { get; set; }
        public T Payload { get; set; }
    }
}