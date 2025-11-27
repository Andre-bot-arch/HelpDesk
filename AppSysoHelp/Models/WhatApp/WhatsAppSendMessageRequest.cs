using System.Text.Json.Serialization;

namespace AppSysoHelp.Models.WhatApp
{
    public class WhatsAppSendMessageRequest
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonPropertyName("recipient_type")]
        public string RecipientType { get; set; } = "individual";

        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public TextMessage? Text { get; set; }

        [JsonPropertyName("interactive")]
        public InteractiveMessage? Interactive { get; set; }
    }

    // Mensagem de texto simples
    public class TextMessage
    {
        [JsonPropertyName("preview_url")]
        public bool PreviewUrl { get; set; } = false;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }

    // Mensagem interativa (botões ou lista)
    public class InteractiveMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("header")]
        public InteractiveHeader? Header { get; set; }

        [JsonPropertyName("body")]
        public InteractiveBody Body { get; set; } = new();

        [JsonPropertyName("footer")]
        public InteractiveFooter? Footer { get; set; }

        [JsonPropertyName("action")]
        public InteractiveAction Action { get; set; } = new();
    }

    public class InteractiveHeader
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class InteractiveBody
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class InteractiveFooter
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class InteractiveAction
    {
        [JsonPropertyName("buttons")]
        public List<ActionButton>? Buttons { get; set; }

        [JsonPropertyName("button")]
        public string? Button { get; set; }

        [JsonPropertyName("sections")]
        public List<ActionSection>? Sections { get; set; }
    }

    public class ActionButton
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "reply";

        [JsonPropertyName("reply")]
        public ButtonReply Reply { get; set; } = new();
    }

    public class ButtonReply
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    public class ActionSection
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("rows")]
        public List<SectionRow> Rows { get; set; } = new();
    }

    public class SectionRow
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    // Resposta da API ao enviar mensagem
    public class WhatsAppSendMessageResponse
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonPropertyName("contacts")]
        public List<ResponseContact> Contacts { get; set; } = new();

        [JsonPropertyName("messages")]
        public List<ResponseMessage> Messages { get; set; } = new();
    }

    public class ResponseContact
    {
        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = string.Empty;
    }

    public class ResponseMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }
}
