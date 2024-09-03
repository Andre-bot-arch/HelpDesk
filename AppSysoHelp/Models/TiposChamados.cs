using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class TiposChamados
{
    public int TipoChamadoId { get; set; }

    public string DescricaoTipoChamado { get; set; } = null!;

    public string? Prioridade { get; set; }

    public virtual ICollection<Chamados> Chamados { get; set; } = new List<Chamados>();
}
