using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class LicencasDispositivos
{
    public long DispositivoId { get; set; }

    public DateTime DataAtivacao { get; set; }

    public string Nome { get; set; } = null!;

    public int? FkLicenca { get; set; }

    public string Identificador { get; set; } = null!;

    public virtual Licencas? FkLicencaNavigation { get; set; }
}
