using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class ChamadosCategoria
{
    public long CategoriaId { get; set; }

    public string DescricaoTipoChamado { get; set; } = null!;

    public bool Status { get; set; }

    public virtual ICollection<ChamadosSubCategoria> ChamadosSubCategoria { get; set; } = new List<ChamadosSubCategoria>();
}
