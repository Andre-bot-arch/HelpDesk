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

        // ✅ ADICIONAR ESTAS PROPRIEDADES PARA MÍDIA
        [JsonPropertyName("image")]
        public WhatsAppMediaMessage? Image { get; set; }

        [JsonPropertyName("audio")]
        public WhatsAppMediaMessage? Audio { get; set; }

        [JsonPropertyName("voice")]
        public WhatsAppMediaMessage? Voice { get; set; }

        [JsonPropertyName("video")]
        public WhatsAppMediaMessage? Video { get; set; }

        [JsonPropertyName("document")]
        public WhatsAppDocumentMessage? Document { get; set; }

        [JsonPropertyName("sticker")]
        public WhatsAppMediaMessage? Sticker { get; set; }

        [JsonPropertyName("location")]
        public WhatsAppLocationMessage? Location { get; set; }

        [JsonPropertyName("contacts")]
        public List<WhatsAppContactMessage>? Contacts { get; set; }
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

    // ✅ CLASSES PARA MÍDIA (IMAGEM, ÁUDIO, VÍDEO, ETC.)
    public class WhatsAppMediaMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        [JsonPropertyName("sha256")]
        public string? Sha256 { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }
    }

    // ✅ CLASSE PARA DOCUMENTOS (PDF, WORD, ETC.)
    public class WhatsAppDocumentMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        [JsonPropertyName("sha256")]
        public string? Sha256 { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }
    }

    // ✅ CLASSE PARA LOCALIZAÇÃO
    public class WhatsAppLocationMessage
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }

    // ✅ CLASSE PARA CONTATOS COMPARTILHADOS
    public class WhatsAppContactMessage
    {
        [JsonPropertyName("name")]
        public WhatsAppContactName? Name { get; set; }

        [JsonPropertyName("phones")]
        public List<WhatsAppContactPhone>? Phones { get; set; }
    }

    public class WhatsAppContactName
    {
        [JsonPropertyName("formatted_name")]
        public string FormattedName { get; set; } = string.Empty;

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
    }

    public class WhatsAppContactPhone
    {
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("wa_id")]
        public string? WaId { get; set; }
    }

    public class WhatsAppStatus
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;
    }
}