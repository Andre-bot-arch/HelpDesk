using System.Text.Json.Serialization;

namespace AppSysoHelp.Models.WhatApp
{
    public class WhatsAppWebhookRequest
    {
        public string Object { get; set; } = string.Empty;
        public List<WhatsAppEntry> Entry { get; set; } = new();
    }

    public class WhatsAppEntry
    {
        public string Id { get; set; } = string.Empty;
        public List<WhatsAppChange> Changes { get; set; } = new();
    }

    public class WhatsAppChange
    {
        public WhatsAppValue Value { get; set; } = new();
        public string Field { get; set; } = string.Empty;
    }

    public class WhatsAppValue
    {
        public string MessagingProduct { get; set; } = string.Empty;
        public WhatsAppMetadata Metadata { get; set; } = new();
        public List<WhatsAppContact> Contacts { get; set; } = new();
        public List<WhatsAppMessage> Messages { get; set; } = new();
        public List<WhatsAppStatus> Statuses { get; set; } = new();
    }

    public class WhatsAppMetadata
    {
        public string DisplayPhoneNumber { get; set; } = string.Empty;
        public string PhoneNumberId { get; set; } = string.Empty;
    }

    public class WhatsAppContact
    {
        public WhatsAppProfile Profile { get; set; } = new();
        public string WaId { get; set; } = string.Empty;
    }

    public class WhatsAppProfile
    {
        public string Name { get; set; } = string.Empty;
    }

    public class WhatsAppMessage
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public WhatsAppTextMessage? Text { get; set; }

        [JsonPropertyName("interactive")]
        public WhatsAppInteractiveMessage? Interactive { get; set; }

        [JsonPropertyName("button")]
        public WhatsAppButtonMessage? Button { get; set; }
    }

    public class WhatsAppTextMessage
    {
        public string Body { get; set; } = string.Empty;
    }

    public class WhatsAppInteractiveMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("button_reply")]
        public WhatsAppButtonReply? ButtonReply { get; set; }

        [JsonPropertyName("list_reply")]
        public WhatsAppListReply? ListReply { get; set; }
    }

    public class WhatsAppButtonReply
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    public class WhatsAppListReply
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class WhatsAppButtonMessage
    {
        public string Payload { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class WhatsAppStatus
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;
    }
}
