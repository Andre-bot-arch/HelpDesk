using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class TecnicosSupervisores
{
    public long PkId { get; set; }

    public string NomeCompleto { get; set; } = null!;

    public string Cpf { get; set; } = null!;

    public string? TelefoneContato { get; set; }

    public string? EmailContato { get; set; }

    public string? CargoResponsabilidade { get; set; }

    public virtual ICollection<Atendimentos> Atendimentos { get; set; } = new List<Atendimentos>();

    public virtual ICollection<Chamados> ChamadosFkAtendenteNavigation { get; set; } = new List<Chamados>();

    public virtual ICollection<Chamados> ChamadosFkTecnico { get; set; } = new List<Chamados>();

    public virtual ICollection<Time> Time { get; set; } = new List<Time>();

    public virtual ICollection<Treinamento> TreinamentoFkAtendenteNavigation { get; set; } = new List<Treinamento>();

    public virtual ICollection<Treinamento> TreinamentoFkTecnico { get; set; } = new List<Treinamento>();

    public virtual ICollection<TreinamentoTempo> TreinamentoTempo { get; set; } = new List<TreinamentoTempo>();
}
