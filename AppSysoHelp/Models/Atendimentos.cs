using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Atendimentos
{
    public long AtendimentoId { get; set; }

    public long FkChamadoId { get; set; }

    public string ProcedimentosAplicados { get; set; } = null!;

    public long? FkTecnicoId { get; set; }

    public DateTime? NovaDataAtendimento { get; set; }

    public bool AtendimentoEncerrado { get; set; }

    public string? SatisfacaoCliente { get; set; }

    public DateTime? DataAtendimento { get; set; }

    public virtual Chamados FkChamado { get; set; } = null!;

    public virtual TecnicosSupervisores? FkTecnico { get; set; }
}
