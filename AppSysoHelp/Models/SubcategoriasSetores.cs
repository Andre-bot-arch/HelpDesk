using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class SubcategoriasSetores
{
    public int SubcategoriaId { get; set; }

    public string NomeSubcategoria { get; set; } = null!;

    public int? FkSetorId { get; set; }

    public virtual SetoresChamados? FkSetor { get; set; }
}
