using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class SituacoesChamados
{
    public int SituacaoChamadoId { get; set; }

    public string DescricaoSituacao { get; set; } = null!;

    public virtual ICollection<Chamados> Chamados { get; set; } = new List<Chamados>();
}
