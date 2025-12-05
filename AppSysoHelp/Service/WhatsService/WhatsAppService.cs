using AppSysoHelp.Models.WhatApp;
using System.Text;
using System.Text.Json;

namespace AppSysoHelp.Service.WhatsService
{
    public class WhatsAppService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly string _accessToken;
        private readonly string _phoneNumberId;
        private readonly string _apiBaseUrl;
        private readonly string _apiVersion;

        public WhatsAppService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            _accessToken = _configuration["WhatsApp:AccessToken"] ?? "";
            _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"] ?? "";
            _apiBaseUrl = _configuration["WhatsApp:ApiBaseUrl"] ?? "";
            _apiVersion = _configuration["WhatsApp:ApiVersion"] ?? "";
        }

        // Enviar mensagem de texto simples
        public async Task<bool> SendTextMessageAsync(string to, string message)
        {
            try
            {
                var request = new WhatsAppSendMessageRequest
                {
                    To = to,
                    Type = "text",
                    Text = new TextMessage
                    {
                        Body = message,
                        PreviewUrl = false
                    }
                };

                return await SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem de texto para {To}", to);
                return false;
            }
        }

        // Enviar mensagem com botões (até 3 botões)
        public async Task<bool> SendButtonMessageAsync(
            string to,
            string bodyText,
            List<(string id, string title)> buttons,
            string? headerText = null,
            string? footerText = null)
        {
            try
            {
                if (buttons.Count > 3)
                {
                    _logger.LogWarning("WhatsApp aceita no máximo 3 botões. Enviando apenas os 3 primeiros.");
                    buttons = buttons.Take(3).ToList();
                }

                var request = new WhatsAppSendMessageRequest
                {
                    To = to,
                    Type = "interactive",
                    Interactive = new InteractiveMessage
                    {
                        Type = "button",
                        Header = string.IsNullOrEmpty(headerText) ? null : new InteractiveHeader
                        {
                            Type = "text",
                            Text = headerText
                        },
                        Body = new InteractiveBody
                        {
                            Text = bodyText
                        },
                        Footer = string.IsNullOrEmpty(footerText) ? null : new InteractiveFooter
                        {
                            Text = footerText
                        },
                        Action = new InteractiveAction
                        {
                            Buttons = buttons.Select(b => new ActionButton
                            {
                                Type = "reply",
                                Reply = new ButtonReply
                                {
                                    Id = b.id,
                                    Title = b.title
                                }
                            }).ToList()
                        }
                    }
                };

                return await SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem com botões para {To}", to);
                return false;
            }
        }

        // Enviar mensagem com lista (menu)
        public async Task<bool> SendListMessageAsync(
            string to,
            string bodyText,
            string buttonText,
            List<(string id, string title)> listItems,
            string? headerText = null,
            string? footerText = null,
            string sectionTitle = "Opções")
        {
            try
            {
                if (listItems.Count > 10)
                {
                    _logger.LogWarning("WhatsApp aceita no máximo 10 itens por seção. Enviando apenas os 10 primeiros.");
                    listItems = listItems.Take(10).ToList();
                }

                var request = new WhatsAppSendMessageRequest
                {
                    To = to,
                    Type = "interactive",
                    Interactive = new InteractiveMessage
                    {
                        Type = "list",
                        Header = string.IsNullOrEmpty(headerText) ? null : new InteractiveHeader
                        {
                            Type = "text",
                            Text = headerText
                        },
                        Body = new InteractiveBody
                        {
                            Text = bodyText
                        },
                        Footer = string.IsNullOrEmpty(footerText) ? null : new InteractiveFooter
                        {
                            Text = footerText
                        },
                        Action = new InteractiveAction
                        {
                            Button = buttonText,
                            Sections = new List<ActionSection>
                            {
                                new ActionSection
                                {
                                    Title = sectionTitle,
                                    Rows = listItems.Select(item => new SectionRow
                                    {
                                        Id = item.id,
                                        Title = item.title
                                        //Description = item.description
                                    }).ToList()
                                }
                            }
                        }
                    }
                };

                return await SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem com lista para {To}", to);
                return false;
            }
        }

        /// <summary>
        /// Enviar mensagem com lista de múltiplas seções (até 10 seções de 10 itens cada)
        /// </summary>
        public async Task<bool> SendMultiSectionListMessageAsync(
            string to,
            string bodyText,
            string buttonText,
            List<(string sectionTitle, List<(string id, string title)> items)> sections,
            string? headerText = null,
            string? footerText = null)
        {
            try
            {
                // Validações
                if (sections == null || !sections.Any())
                {
                    _logger.LogWarning("Nenhuma seção fornecida para lista");
                    return false;
                }

                if (sections.Count > 10)
                {
                    _logger.LogWarning("WhatsApp aceita no máximo 10 seções.  Enviando apenas as 10 primeiras.");
                    sections = sections.Take(10).ToList();
                }

                // 🔍 LOG DE DEBUG - Ver quantos itens cada seção tem
                _logger.LogInformation("📋 Preparando lista com {Count} seções:", sections.Count);
                foreach (var section in sections)
                {
                    _logger.LogInformation("  - Seção '{Title}': {ItemCount} itens",
                        section.sectionTitle, section.items.Count);
                }

                var request = new WhatsAppSendMessageRequest
                {
                    To = to,
                    Type = "interactive",
                    Interactive = new InteractiveMessage
                    {
                        Type = "list",
                        Header = string.IsNullOrEmpty(headerText) ? null : new InteractiveHeader
                        {
                            Type = "text",
                            Text = headerText
                        },
                        Body = new InteractiveBody
                        {
                            Text = bodyText
                        },
                        Footer = string.IsNullOrEmpty(footerText) ? null : new InteractiveFooter
                        {
                            Text = footerText
                        },
                        Action = new InteractiveAction
                        {
                            Button = buttonText,
                            Sections = sections.Select(section =>
                            {
                                var limitedItems = section.items.Take(10).ToList();

                                // 🔥 GARANTIR QUE TÍTULO TEM MAX 24 CARACTERES
                                var sectionTitle = section.sectionTitle.Length > 24
                                    ? section.sectionTitle.Substring(0, 24)
                                    : section.sectionTitle;

                                _logger.LogDebug("  → Seção '{Title}' ({Length} chars) enviando {Count} itens",
                                    sectionTitle, sectionTitle.Length, limitedItems.Count);

                                return new ActionSection
                                {
                                    Title = sectionTitle,
                                    Rows = limitedItems.Select(item => new SectionRow
                                    {
                                        Id = item.id,
                                        Title = item.title.Length > 24
                                            ? item.title.Substring(0, 24)
                                            : item.title
                                    }).ToList()
                                };
                            }).ToList()
                        }
                    }
                };

                var totalItems = sections.Sum(s => Math.Min(s.items.Count, 10));
                _logger.LogInformation("✅ Enviando lista com {SectionCount} seções e {ItemCount} itens total para {To}",
                    sections.Count, totalItems, to);

                return await SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem com múltiplas seções para {To}", to);
                return false;
            }
        }



        // Método principal que faz o POST para a API da Meta
        private async Task<bool> SendMessageAsync(WhatsAppSendMessageRequest request)
        {
            try
            {
                var url = $"{_apiBaseUrl}/{_apiVersion}/{_phoneNumberId}/messages";

                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                _logger.LogInformation("Enviando mensagem para WhatsApp API: {Url}", url);
                _logger.LogDebug("Payload: {Payload}", jsonContent);

                // CRIAR HttpClient aqui ⬇️
                using var httpClient = _httpClient.CreateClient();

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                httpRequest.Headers.Add("Authorization", $"Bearer {_accessToken}");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Mensagem enviada com sucesso. Response: {Response}", responseContent);
                    return true;
                }
                else
                {
                    _logger.LogError("Erro ao enviar mensagem. Status: {Status}, Response: {Response}",
                        response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao enviar mensagem para WhatsApp API");
                return false;
            }
        }
    }
}
