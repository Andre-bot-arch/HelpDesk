using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class Contratos
{
    public long ContratoId { get; set; }

    public long FkPlataformaId { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly DataFim { get; set; }

    public string SituacaoContrato { get; set; } = null!;

    public decimal Valor { get; set; }

    public string IdContrato { get; set; } = null!;

    public string? DescricaoContrato { get; set; }

    public int PontosContratados { get; set; }

    public long? FkClienteId { get; set; }

    public virtual PlataformasContratos FkPlataforma { get; set; } = null!;
}
