using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class VwAtendimentos
{
    public long ProtocoloChamado { get; set; }

    public string Cliente { get; set; } = null!;

    public string Tecnico { get; set; } = null!;

    public DateTime? DataAbertura { get; set; }

    public DateTime? DataAtendimento { get; set; }

    public DateTime? DataFechamento { get; set; }

    public string Situacao { get; set; } = null!;

    public string? Categoria { get; set; }

    public string? SubCategoria { get; set; }

    public string? MotivoAbertura { get; set; }

    public string ProcedimentosAplicados { get; set; } = null!;
}
