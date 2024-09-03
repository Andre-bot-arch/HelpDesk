using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Clientes
{
    public long ClienteId { get; set; }

    public string NomeCliente { get; set; } = null!;

    public string? TelefoneCliente { get; set; }

    public string? Setor { get; set; }

    public virtual ICollection<Chamados> Chamados { get; set; } = new List<Chamados>();
}
