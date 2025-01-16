using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Chamados
{
    public long ChamadoId { get; set; }

    public long? FkClienteId { get; set; }

    public string Contato { get; set; } = null!;

    public string? TelefoneContato { get; set; }

    public long FkTipoChamadoId { get; set; }

    public string? DescricaoCompleta { get; set; }

    public long FkTecnicoId { get; set; }

    public DateTime? DataAgendamento { get; set; }

    public long FkSituacaoChamadoId { get; set; }

    public long? FkPlataforma { get; set; }

    public DateTime? DataCriacao { get; set; }

    public long? FkAtendente { get; set; }

    public string? Prioridade { get; set; }

    public long FkSetores { get; set; }

    public DateTime? DataFechamento { get; set; }

    public long? FkSubCategoria { get; set; }

    public TimeOnly? HorasContratada { get; set; }

    public TimeOnly? HorasSaldo { get; set; }

    public virtual ICollection<Atendimentos> Atendimentos { get; set; } = new List<Atendimentos>();

    public virtual TecnicosSupervisores? FkAtendenteNavigation { get; set; }

    public virtual Clientes? FkCliente { get; set; }

    public virtual SetoresChamados FkSetoresNavigation { get; set; } = null!;

    public virtual SituacoesChamados FkSituacaoChamado { get; set; } = null!;

    public virtual TecnicosSupervisores FkTecnico { get; set; } = null!;

    public virtual TiposChamados FkTipoChamado { get; set; } = null!;
}
