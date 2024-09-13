using System;
using System.Collections.Generic;

namespace AppSysoHelp.Models;

public partial class SysoCloud
{
    public long PkId { get; set; }

    public string? Descricao { get; set; }

    public string? Horario { get; set; }

    public string? TipoBackup { get; set; }

    public string? Extensao { get; set; }

    public string? CaminhoDownload { get; set; }

    public string? CaminhoUpload { get; set; }

    public string? CaminhoBuscaArquivo { get; set; }

    public string? ExecutarAntesBackup { get; set; }

    public bool? Segunda { get; set; }

    public bool? Terca { get; set; }

    public bool? Quarta { get; set; }

    public bool? Quinta { get; set; }

    public bool? Sexta { get; set; }

    public bool? Sabado { get; set; }

    public bool? Domingo { get; set; }

    public string? ExecutarAposBackup { get; set; }

    public long FkClienteId { get; set; }

    public long FkContratoId { get; set; }

    public virtual Clientes FkCliente { get; set; } = null!;

    public virtual Contratos FkContrato { get; set; } = null!;
}
