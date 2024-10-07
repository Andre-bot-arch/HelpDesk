using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class ChamadosSubCategoria
{
    public long SubCategoriaId { get; set; }

    public long FkCategoria { get; set; }

    public string Descricao { get; set; } = null!;

    public string Prioridade { get; set; } = null!;

    public long Complexidade { get; set; }

    public virtual ChamadosCategoria FkCategoriaNavigation { get; set; } = null!;
}
