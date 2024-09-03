using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class PlataformasContratos
{
    public long PlataformaId { get; set; }

    public string NomePlataforma { get; set; } = null!;

    public string? Descricao { get; set; }

    public virtual ICollection<Contratos> Contratos { get; set; } = new List<Contratos>();
}
