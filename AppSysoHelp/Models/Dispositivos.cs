using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Dispositivos
{
    public long Id { get; set; }

    public string Equipamento { get; set; } = null!;

    public string Chave { get; set; } = null!;

    public DateTime DataCriacao { get; set; }

    public string? Apelido { get; set; }

    public DateTime? UltimoAcesso { get; set; }

    public long FkLicenca { get; set; }

    public virtual Licencas FkLicencaNavigation { get; set; } = null!;
}
