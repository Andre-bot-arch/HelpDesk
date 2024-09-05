using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Clientes
{
    public long ClienteId { get; set; }

    public string NomeCliente { get; set; } = null!;

    public string? TelefoneCliente { get; set; }

    public string? Documento { get; set; }

    public string? Cep { get; set; }

    public string? Logradouro { get; set; }

    public string? Numero { get; set; }

    public string? Bairro { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }

    public virtual ICollection<Chamados> Chamados { get; set; } = new List<Chamados>();

    public virtual ICollection<Contratos> Contratos { get; set; } = new List<Contratos>();
}
