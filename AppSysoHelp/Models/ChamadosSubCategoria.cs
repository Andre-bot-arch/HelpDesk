using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class ChamadosSubCategoria
{
    public long SubCategoriaId { get; set; }

    public long FkCategoria { get; set; }

    public string DescricaoTipoChamado { get; set; } = null!;

    public string Prioridade { get; set; } = null!;

    public int Peso { get; set; }
}
