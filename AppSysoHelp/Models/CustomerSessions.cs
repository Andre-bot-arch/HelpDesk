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

        /// <summary>
        /// Fluxo atual: "creating_ticket", "waiting_category", etc
        /// </summary>
        public string? CurrentFlow { get; set; }

        /// <summary>
        /// Dados temporários do fluxo em JSON
        /// Ex: {"customerName":"João", "categoriaId":5}
        /// </summary>
        public string? FlowData { get; set; }

        /// <summary>
        /// ID do chamado vinculado (quando criado via WhatsApp)
        /// </summary>
        public long? LinkedTicketId { get; set; }

        //verificacao se é atendimento humano
        public bool IsHumanAttendance { get; set; }

        // Navegação
        public virtual Chamados? LinkedTicket { get; set; }
    }
}
