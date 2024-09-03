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

    public virtual ICollection<Chamados> Chamados { get; set; } = new List<Chamados>();
}
