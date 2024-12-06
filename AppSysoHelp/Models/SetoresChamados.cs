using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class SetoresChamados
{
    public long SetorId { get; set; }

    public string NomeSetor { get; set; } = null!;

    public virtual ICollection<Chamados> Chamados { get; set; } = new List<Chamados>();

    public virtual ICollection<Treinamento> Treinamento { get; set; } = new List<Treinamento>();
}
