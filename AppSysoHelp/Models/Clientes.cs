using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Clientes
{
    public long ClienteId { get; set; }

    public string NomeCliente { get; set; } = null!;

    public string? Fantasia { get; set; }

    public string? Documento { get; set; }

    public string? TelefoneCliente { get; set; }

    public string? WhatsApp { get; set; }

    public string? Email { get; set; }

    public string? Cep { get; set; }

    public string? Logradouro { get; set; }

    public string? Bairro { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }

    public bool Situacao { get; set; }

    public string? IdSolution { get; set; }

    public virtual ICollection<Chamados> Chamados { get; set; } = new List<Chamados>();

    public virtual ICollection<Contratos> Contratos { get; set; } = new List<Contratos>();

    public virtual ICollection<SysoCloud> SysoCloud { get; set; } = new List<SysoCloud>();
}
