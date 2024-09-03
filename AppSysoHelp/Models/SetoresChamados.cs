using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class SetoresChamados
{
    public int SetorId { get; set; }

    public string NomeSetor { get; set; } = null!;

    public virtual ICollection<SubcategoriasSetores> SubcategoriasSetores { get; set; } = new List<SubcategoriasSetores>();
}
