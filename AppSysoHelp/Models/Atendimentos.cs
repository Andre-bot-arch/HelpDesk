using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Atendimentos
{
    public int AtendimentoId { get; set; }

    public int ChamadoId { get; set; }

    public string ProcedimentosAplicados { get; set; } = null!;

    public int? TecnicoId { get; set; }

    public DateTime? NovaDataAtendimento { get; set; }

    public bool AtendimentoEncerrado { get; set; }

    public string? SatisfacaoCliente { get; set; }

    public virtual Chamados Chamado { get; set; } = null!;

    public virtual TecnicosSupervisores? Tecnico { get; set; }
}
