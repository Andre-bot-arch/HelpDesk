using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class TotalizadorChamadosPorTecnico
{
    public int? Total { get; set; }

    public string? Prioridade { get; set; }

    public string NomeCompleto { get; set; } = null!;
}
