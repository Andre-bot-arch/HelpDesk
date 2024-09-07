using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Licencas
{
    public int LicencaId { get; set; }

    public DateOnly DataAtivacao { get; set; }

    public string Urlacesso { get; set; } = null!;

    public string? Descricao { get; set; }

    public string? Modelo { get; set; }

    public string NumeroSerie { get; set; } = null!;

    public string? RegistroDispositivos { get; set; }

    public string? LogAcesso { get; set; }

    public long? FkContratoId { get; set; }

    public bool? Ativo { get; set; }

    public string? Hash { get; set; }

    public virtual Contratos? FkContrato { get; set; }

    public virtual ICollection<LicencasDispositivos> LicencasDispositivos { get; set; } = new List<LicencasDispositivos>();
}
