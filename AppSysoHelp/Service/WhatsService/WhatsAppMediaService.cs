using System.Net.Http.Headers;
using System.Text.Json;

namespace AppSysoHelp.Service.WhatsService
{
    public class WhatsAppMediaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppMediaService> _logger;
        private readonly string _mediaStoragePath;

        public WhatsAppMediaService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WhatsAppMediaService> logger,
            IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            // Pasta para salvar mídias
            _mediaStoragePath = Path.Combine(env.WebRootPath, "whatsapp-media");

            // Criar pasta se não existir
            if (!Directory.Exists(_mediaStoragePath))
            {
                Directory.CreateDirectory(_mediaStoragePath);
            }
        }

        /// <summary>
        /// Baixa mídia do WhatsApp e salva localmente
        /// </summary>
        public async Task<string?> DownloadAndSaveMediaAsync(string mediaId, string mediaType)
        {
            try
            {
                var accessToken = _configuration["WhatsApp:AccessToken"];
                var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Access Token do WhatsApp não configurado");
                    return null;
                }

                // 1. Obter URL da mídia
                var mediaUrl = await GetMediaUrlAsync(mediaId, accessToken);
                if (string.IsNullOrEmpty(mediaUrl))
                {
                    return null;
                }

                // 2. Baixar a mídia
                var mediaBytes = await DownloadMediaBytesAsync(mediaUrl, accessToken);
                if (mediaBytes == null || mediaBytes.Length == 0)
                {
                    return null;
                }

                // 3. Determinar extensão do arquivo
                var extension = GetFileExtension(mediaType, mediaBytes);

                // 4. Salvar localmente
                var fileName = $"{mediaId}_{DateTime.Now.Ticks}{extension}";
                var filePath = Path.Combine(_mediaStoragePath, fileName);

                await File.WriteAllBytesAsync(filePath, mediaBytes);

                _logger.LogInformation("✅ Mídia salva:  {FileName}", fileName);

                // 5. Retornar URL relativa
                return $"/whatsapp-media/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao baixar mídia {MediaId}", mediaId);
                return null;
            }
        }

        /// <summary>
        /// Obtém URL da mídia via API do WhatsApp
        /// </summary>
        private async Task<string?> GetMediaUrlAsync(string mediaId, string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(
                    $"https://graph.facebook.com/v21.0/{mediaId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao obter URL da mídia: {Status}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var mediaData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);

                if (mediaData.TryGetProperty("url", out var urlProperty))
                {
                    return urlProperty.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter URL da mídia");
                return null;
            }
        }

        /// <summary>
        /// Baixa bytes da mídia
        /// </summary>
        private async Task<byte[]?> DownloadMediaBytesAsync(string mediaUrl, string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(mediaUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao baixar mídia: {Status}", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar bytes da mídia");
                return null;
            }
        }

        /// <summary>
        /// Determina extensão do arquivo baseado no tipo e bytes
        /// </summary>
        private string GetFileExtension(string mediaType, byte[] bytes)
        {
            // Verificar pelos magic numbers (primeiros bytes do arquivo)
            if (bytes.Length >= 4)
            {
                // PNG
                if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                    return ".png";

                // JPEG
                if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                    return ".jpg";

                // GIF
                if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                    return ".gif";

                // WebP
                if (bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                    return ".webp";

                // PDF
                if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
                    return ".pdf";
            }

            // Fallback baseado no tipo
            return mediaType?.ToLower() switch
            {
                "image" => ".jpg",
                "audio" => ".ogg",
                "voice" => ".ogg",
                "video" => ".mp4",
                "document" => ".pdf",
                "sticker" => ".webp",
                _ => ".bin"
            };
        }
    }
}
