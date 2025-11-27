namespace AppSysoHelp.Models
{
    public class CustomerSessions
    {
        public int Id { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public int State { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastInteraction { get; set; }

        public string? AssignedAgent { get; set; }

        public string? AdditionalData { get; set; }
    }
}
