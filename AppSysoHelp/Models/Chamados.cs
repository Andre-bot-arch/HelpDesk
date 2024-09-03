using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Chamados
{
    public int ChamadoId { get; set; }

    public int ClienteId { get; set; }

    public string Contato { get; set; } = null!;

    public string? TelefoneContato { get; set; }

    public int TipoChamadoId { get; set; }

    public string? DescricaoCompleta { get; set; }

    public int TecnicoId { get; set; }

    public DateOnly? DataAgendamento { get; set; }

    public int SituacaoChamadoId { get; set; }

    public virtual ICollection<Atendimentos> Atendimentos { get; set; } = new List<Atendimentos>();

    public virtual Clientes Cliente { get; set; } = null!;

    public virtual SituacoesChamados SituacaoChamado { get; set; } = null!;

    public virtual TecnicosSupervisores Tecnico { get; set; } = null!;

    public virtual TiposChamados TipoChamado { get; set; } = null!;
}
