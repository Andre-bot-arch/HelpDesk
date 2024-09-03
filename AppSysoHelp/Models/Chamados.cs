using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Chamados
{
    public long ChamadoId { get; set; }

    public long FkClienteId { get; set; }

    public string Contato { get; set; } = null!;

    public string? TelefoneContato { get; set; }

    public long FkTipoChamadoId { get; set; }

    public string? DescricaoCompleta { get; set; }

    public long FkTecnicoId { get; set; }

    public DateOnly? DataAgendamento { get; set; }

    public long FkSituacaoChamadoId { get; set; }

    public virtual ICollection<Atendimentos> Atendimentos { get; set; } = new List<Atendimentos>();

    public virtual Clientes FkCliente { get; set; } = null!;

    public virtual SituacoesChamados FkSituacaoChamado { get; set; } = null!;

    public virtual TecnicosSupervisores FkTecnico { get; set; } = null!;

    public virtual TiposChamados FkTipoChamado { get; set; } = null!;
}
