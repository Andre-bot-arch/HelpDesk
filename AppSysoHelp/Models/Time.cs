using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Time
{
    public long PkId { get; set; }

    public string Nome { get; set; } = null!;

    public string Funcao { get; set; } = null!;

    public string? CaminhoImagem { get; set; }

    public bool Ativo { get; set; }

    public long? FkTecnico { get; set; }

    public virtual TecnicosSupervisores? FkTecnicoNavigation { get; set; }
}
