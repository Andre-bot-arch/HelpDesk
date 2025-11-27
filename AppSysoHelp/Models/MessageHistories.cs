namespace AppSysoHelp.Models
{
    public class MessageHistories
    {
        public int Id { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string Direction { get; set; } = null!;

        public string MessageType { get; set; } = null!;

        public string? MessageContent { get; set; }

        public DateTime Timestamp { get; set; }

        public string? SentBy { get; set; }

        public string? WhatsAppMessageId { get; set; }
    }
}
