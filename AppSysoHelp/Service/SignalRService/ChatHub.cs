using k8s.KubeConfigModels;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace AppSysoHelp.Service.SignalRService
{
    /// <summary>
    /// Hub SignalR para comunicação em tempo real entre técnicos e clientes WhatsApp
    /// </summary>
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Técnico entra no grupo do chamado (quando abre a tela de atendimento)
        /// </summary>
        public async Task JoinChamadoGroup(string chamadoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chamado_{chamadoId}");

            _logger.LogInformation("✅ Técnico {ConnectionId} entrou no grupo do chamado #{ChamadoId}",
                Context.ConnectionId, chamadoId);
        }

        /// <summary>
        /// Técnico sai do grupo do chamado (quando fecha a tela)
        /// </summary>
        public async Task LeaveChamadoGroup(string chamadoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chamado_{chamadoId}");

            _logger.LogInformation("❌ Técnico {ConnectionId} saiu do grupo do chamado #{ChamadoId}",
                Context.ConnectionId, chamadoId);
        }

        /// <summary>
        /// Evento disparado quando técnico conecta
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("🔌 Nova conexão SignalR:  {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Evento disparado quando técnico desconecta
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogWarning("🔌 Conexão SignalR encerrada: {ConnectionId}. Erro: {Error}",
                Context.ConnectionId, exception?.Message ?? "Nenhum");

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Entrar no grupo de um telefone específico (para chat multi-conversas)
        /// </summary>
        public async Task JoinPhoneGroup(string phoneNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"phone_{phoneNumber}");
            _logger.LogInformation("👤 Usuário {ConnectionId} entrou no grupo do telefone {Phone}",
                Context.ConnectionId, phoneNumber);
        }

        /// <summary>
        /// Sair do grupo de um telefone específico
        /// </summary>
        public async Task LeavePhoneGroup(string phoneNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"phone_{phoneNumber}");
            _logger.LogInformation("👤 Usuário {ConnectionId} saiu do grupo do telefone {Phone}",
                Context.ConnectionId, phoneNumber);
        }


    }
}
