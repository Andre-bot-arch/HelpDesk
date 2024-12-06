using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class TreinamentoTempo
{
    public long PkId { get; set; }

    public string DescricaoDoTreinamento { get; set; } = null!;

    public double Tempo { get; set; }

    public long FkTecnico { get; set; }

    public long FkTreinamento { get; set; }

    public virtual TecnicosSupervisores FkTecnicoNavigation { get; set; } = null!;

    public virtual Treinamento FkTreinamentoNavigation { get; set; } = null!;
}
