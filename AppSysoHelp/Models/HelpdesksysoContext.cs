using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Models;

public partial class HelpdesksysoContext : DbContext
{
    public HelpdesksysoContext()
    {
    }

    public HelpdesksysoContext(DbContextOptions<HelpdesksysoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Atendimentos> Atendimentos { get; set; }

    public virtual DbSet<Chamados> Chamados { get; set; }

    public virtual DbSet<ChamadosCategoria> ChamadosCategoria { get; set; }

    public virtual DbSet<ChamadosSubCategoria> ChamadosSubCategoria { get; set; }

    public virtual DbSet<Clientes> Clientes { get; set; }

    public virtual DbSet<Contratos> Contratos { get; set; }

    public virtual DbSet<Dispositivos> Dispositivos { get; set; }

    public virtual DbSet<Licencas> Licencas { get; set; }

    public virtual DbSet<PlataformasContratos> PlataformasContratos { get; set; }

    public virtual DbSet<SetoresChamados> SetoresChamados { get; set; }

    public virtual DbSet<SituacoesChamados> SituacoesChamados { get; set; }

    public virtual DbSet<SysoCloud> SysoCloud { get; set; }

    public virtual DbSet<TecnicosSupervisores> TecnicosSupervisores { get; set; }

    public virtual DbSet<Time> Time { get; set; }

    public virtual DbSet<TiposChamados> TiposChamados { get; set; }

    public virtual DbSet<TotalizadorChamadosPorTecnico> TotalizadorChamadosPorTecnico { get; set; }

    public virtual DbSet<Treinamento> Treinamento { get; set; }

    public virtual DbSet<TreinamentoTempo> TreinamentoTempo { get; set; }

    public virtual DbSet<VwAtendimentos> VwAtendimentos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=mssql2017.hostingzone.com.br,1433;Initial Catalog=helpdesksyso;Persist Security Info=True;User ID=helpdesk;Password=syso@3680;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("helpdesk")
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Atendimentos>(entity =>
        {
            entity.HasKey(e => e.AtendimentoId).HasName("PK__Atendime__FD23D5491E91D5CB");

            entity.ToTable("Atendimentos", "dbo");

            entity.Property(e => e.CaminhoDaImagem).IsUnicode(false);
            entity.Property(e => e.DataAtendimento).HasColumnType("datetime");
            entity.Property(e => e.DataFechamento).HasColumnType("datetime");
            entity.Property(e => e.FkChamadoId).HasColumnName("Fk_ChamadoId");
            entity.Property(e => e.FkTecnicoId).HasColumnName("Fk_TecnicoId");
            entity.Property(e => e.NovaDataAtendimento).HasColumnType("datetime");
            entity.Property(e => e.SatisfacaoCliente).HasMaxLength(500);

            entity.HasOne(d => d.FkChamado).WithMany(p => p.Atendimentos)
                .HasForeignKey(d => d.FkChamadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Atendimen__Chama__75A278F5");

            entity.HasOne(d => d.FkTecnico).WithMany(p => p.Atendimentos)
                .HasForeignKey(d => d.FkTecnicoId)
                .HasConstraintName("FK__Atendimen__Tecni__76969D2E");
        });

        modelBuilder.Entity<Chamados>(entity =>
        {
            entity.HasKey(e => e.ChamadoId).HasName("PK__Chamados__A9D1243BCA664A93");

            entity.ToTable("Chamados", "dbo");

            entity.Property(e => e.Contato).HasMaxLength(150);
            entity.Property(e => e.DataAgendamento).HasColumnType("datetime");
            entity.Property(e => e.DataCriacao)
                .HasColumnType("datetime")
                .HasColumnName("Data_Criacao");
            entity.Property(e => e.DataFechamento).HasColumnType("datetime");
            entity.Property(e => e.FkAtendente).HasColumnName("Fk_Atendente");
            entity.Property(e => e.FkClienteId).HasColumnName("Fk_ClienteId");
            entity.Property(e => e.FkPlataforma).HasColumnName("Fk_Plataforma");
            entity.Property(e => e.FkSetores).HasColumnName("Fk_Setores");
            entity.Property(e => e.FkSituacaoChamadoId).HasColumnName("Fk_SituacaoChamadoId");
            entity.Property(e => e.FkSubCategoria).HasColumnName("Fk_SubCategoria");
            entity.Property(e => e.FkTecnicoId).HasColumnName("Fk_TecnicoId");
            entity.Property(e => e.FkTipoChamadoId).HasColumnName("Fk_TipoChamadoId");
            entity.Property(e => e.Prioridade)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.FkAtendenteNavigation).WithMany(p => p.ChamadosFkAtendenteNavigation)
                .HasForeignKey(d => d.FkAtendente)
                .HasConstraintName("FK_Chamados_TecnicosSupervisores");

            entity.HasOne(d => d.FkCliente).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkClienteId)
                .HasConstraintName("FK__Chamados__Client__6C190EBB");

            entity.HasOne(d => d.FkSetoresNavigation).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkSetores)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chamados_SetoresChamados");

            entity.HasOne(d => d.FkSituacaoChamado).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkSituacaoChamadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chamados__Situac__6EF57B66");

            entity.HasOne(d => d.FkTecnico).WithMany(p => p.ChamadosFkTecnico)
                .HasForeignKey(d => d.FkTecnicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chamados__Tecnic__6E01572D");

            entity.HasOne(d => d.FkTipoChamado).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkTipoChamadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chamados__TipoCh__6D0D32F4");
        });

        modelBuilder.Entity<ChamadosCategoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Chamados__F353C1E5D06C7DAC");

            entity.ToTable("ChamadosCategoria", "dbo");

            entity.Property(e => e.Descricao)
                .HasMaxLength(150)
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<ChamadosSubCategoria>(entity =>
        {
            entity.HasKey(e => e.SubCategoriaId).HasName("PK__Chamados__7DE876121A8D13E0");

            entity.ToTable("ChamadosSubCategoria", "dbo");

            entity.Property(e => e.Descricao).HasMaxLength(150);
            entity.Property(e => e.Prioridade)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Situacao).HasDefaultValue(true);

            entity.HasOne(d => d.FkCategoriaNavigation).WithMany(p => p.ChamadosSubCategoria)
                .HasForeignKey(d => d.FkCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChamadosSubCategoria_ChamadosCategoria");
        });

        modelBuilder.Entity<Clientes>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__Clientes__71ABD08795878164");

            entity.ToTable("Clientes", "dbo");

            entity.Property(e => e.Bairro).IsUnicode(false);
            entity.Property(e => e.Cidade).IsUnicode(false);
            entity.Property(e => e.IdSolution).IsUnicode(false);
            entity.Property(e => e.Logradouro).IsUnicode(false);
            entity.Property(e => e.Uf)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Contratos>(entity =>
        {
            entity.HasKey(e => e.ContratoId).HasName("PK__Contrato__B238E9730AEF188D");

            entity.ToTable("Contratos", "dbo");

            entity.Property(e => e.DescricaoContrato).HasMaxLength(255);
            entity.Property(e => e.FkClienteId).HasColumnName("Fk_ClienteId");
            entity.Property(e => e.FkPlataformaId).HasColumnName("Fk_PlataformaId");
            entity.Property(e => e.IdSolution).IsUnicode(false);
            entity.Property(e => e.SituacaoContrato).HasMaxLength(50);
            entity.Property(e => e.Valor).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.FkCliente).WithMany(p => p.Contratos)
                .HasForeignKey(d => d.FkClienteId)
                .HasConstraintName("FK_Contratos_Clientes");

            entity.HasOne(d => d.FkPlataforma).WithMany(p => p.Contratos)
                .HasForeignKey(d => d.FkPlataformaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contratos__Plata__7D439ABD");
        });

        modelBuilder.Entity<Dispositivos>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disposit__3214EC07B93AECCD");

            entity.ToTable("Dispositivos", "dbo");

            entity.Property(e => e.Apelido).IsUnicode(false);
            entity.Property(e => e.Chave).IsUnicode(false);
            entity.Property(e => e.DataCriacao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Equipamento).IsUnicode(false);

            entity.HasOne(d => d.FkLicencaNavigation).WithMany(p => p.Dispositivos)
                .HasForeignKey(d => d.FkLicenca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dispositivos_Licencas");
        });

        modelBuilder.Entity<Licencas>(entity =>
        {
            entity.HasKey(e => e.LicencaId).HasName("PK__Licencas__28C8BC25B851726F");

            entity.ToTable("Licencas", "dbo");

            entity.HasIndex(e => e.NumeroSerie, "UQ__Licencas__C5455177A4669547").IsUnique();

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Descricao).HasMaxLength(255);
            entity.Property(e => e.FkContratoId).HasColumnName("Fk_ContratoId");
            entity.Property(e => e.Hash).HasMaxLength(20);
            entity.Property(e => e.Modelo).HasMaxLength(100);
            entity.Property(e => e.NumeroSerie).HasMaxLength(100);
            entity.Property(e => e.Urlacesso)
                .HasMaxLength(255)
                .HasColumnName("URLAcesso");

            entity.HasOne(d => d.FkContrato).WithMany(p => p.Licencas)
                .HasForeignKey(d => d.FkContratoId)
                .HasConstraintName("FK_Licencas_Contratos");
        });

        modelBuilder.Entity<PlataformasContratos>(entity =>
        {
            entity.HasKey(e => e.PlataformaId).HasName("PK__Platafor__B83567ED005E86DA");

            entity.ToTable("PlataformasContratos", "dbo");

            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.IdSolution).IsUnicode(false);
            entity.Property(e => e.NomePlataforma).HasMaxLength(100);
        });

        modelBuilder.Entity<SetoresChamados>(entity =>
        {
            entity.HasKey(e => e.SetorId).HasName("PK__SetoresC__26AA3A0368EA4F90");

            entity.ToTable("SetoresChamados", "dbo");

            entity.Property(e => e.NomeSetor).HasMaxLength(100);
        });

        modelBuilder.Entity<SituacoesChamados>(entity =>
        {
            entity.HasKey(e => e.SituacaoChamadoId).HasName("PK__Situacoe__0BB0F7FDA99762E5");

            entity.ToTable("SituacoesChamados", "dbo");

            entity.Property(e => e.DescricaoSituacao).HasMaxLength(100);
        });

        modelBuilder.Entity<SysoCloud>(entity =>
        {
            entity.HasKey(e => e.PkId);

            entity.ToTable("SysoCloud", "dbo");

            entity.Property(e => e.CaminhoBuscaArquivo).IsUnicode(false);
            entity.Property(e => e.CaminhoDownload).IsUnicode(false);
            entity.Property(e => e.CaminhoUpload).IsUnicode(false);
            entity.Property(e => e.DataCreate).HasColumnType("datetime");
            entity.Property(e => e.Descricao).IsUnicode(false);
            entity.Property(e => e.ExecutarAntesBackup).IsUnicode(false);
            entity.Property(e => e.ExecutarAposBackup).IsUnicode(false);
            entity.Property(e => e.Extensao).IsUnicode(false);
            entity.Property(e => e.Horario).IsUnicode(false);
            entity.Property(e => e.TipoBackup).IsUnicode(false);

            entity.HasOne(d => d.FkCliente).WithMany(p => p.SysoCloud)
                .HasForeignKey(d => d.FkClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SysoCloud_Clientes");

            entity.HasOne(d => d.FkContrato).WithMany(p => p.SysoCloud)
                .HasForeignKey(d => d.FkContratoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SysoCloud_Contratos");
        });

        modelBuilder.Entity<TecnicosSupervisores>(entity =>
        {
            entity.HasKey(e => e.PkId).HasName("PK__Tecnicos__A7C03FF834E4F2D5");

            entity.ToTable("TecnicosSupervisores", "dbo");

            entity.HasIndex(e => e.Cpf, "UQ__Tecnicos__C1F897313533D71D").IsUnique();

            entity.Property(e => e.CargoResponsabilidade).HasMaxLength(50);
            entity.Property(e => e.Cpf)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CPF");
            entity.Property(e => e.EmailContato).HasMaxLength(100);
            entity.Property(e => e.NomeCompleto).HasMaxLength(150);
            entity.Property(e => e.TelefoneContato).HasMaxLength(20);
        });

        modelBuilder.Entity<Time>(entity =>
        {
            entity.HasKey(e => e.PkId);

            entity.ToTable("Time", "dbo");

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.CaminhoImagem).IsUnicode(false);
            entity.Property(e => e.Funcao).IsUnicode(false);
            entity.Property(e => e.Nome).IsUnicode(false);
        });

        modelBuilder.Entity<TiposChamados>(entity =>
        {
            entity.HasKey(e => e.TipoChamadoId).HasName("PK__TiposCha__0A7535DCF0A7FCDD");

            entity.ToTable("TiposChamados", "dbo");

            entity.Property(e => e.DescricaoTipoChamado).HasMaxLength(150);
            entity.Property(e => e.Prioridade).HasMaxLength(50);
        });

        modelBuilder.Entity<TotalizadorChamadosPorTecnico>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TotalizadorChamadosPorTecnico");

            entity.Property(e => e.NomeCompleto).HasMaxLength(150);
            entity.Property(e => e.Prioridade)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Total).HasColumnName("total");
        });

        modelBuilder.Entity<Treinamento>(entity =>
        {
            entity.HasKey(e => e.ChamadoId).HasName("PK__Treiname__A9D1243BC3DDD0C6");

            entity.ToTable("Treinamento", "dbo");

            entity.Property(e => e.Contato).HasMaxLength(150);
            entity.Property(e => e.DataAgendamento).HasColumnType("datetime");
            entity.Property(e => e.DataCriacao)
                .HasColumnType("datetime")
                .HasColumnName("Data_Criacao");
            entity.Property(e => e.DataFechamento).HasColumnType("datetime");
            entity.Property(e => e.FkAtendente).HasColumnName("Fk_Atendente");
            entity.Property(e => e.FkClienteId).HasColumnName("Fk_ClienteId");
            entity.Property(e => e.FkPlataforma).HasColumnName("Fk_Plataforma");
            entity.Property(e => e.FkSetores).HasColumnName("Fk_Setores");
            entity.Property(e => e.FkSituacaoChamadoId).HasColumnName("Fk_SituacaoChamadoId");
            entity.Property(e => e.FkSubCategoria).HasColumnName("Fk_SubCategoria");
            entity.Property(e => e.FkTecnicoId).HasColumnName("Fk_TecnicoId");
            entity.Property(e => e.FkTipoChamadoId).HasColumnName("Fk_TipoChamadoId");
            entity.Property(e => e.Horas).HasColumnName("horas");
            entity.Property(e => e.Prioridade)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.FkAtendenteNavigation).WithMany(p => p.TreinamentoFkAtendenteNavigation)
                .HasForeignKey(d => d.FkAtendente)
                .HasConstraintName("FK_Treinamento_TecnicosSupervisores1");

            entity.HasOne(d => d.FkCliente).WithMany(p => p.Treinamento)
                .HasForeignKey(d => d.FkClienteId)
                .HasConstraintName("FK_Treinamento_Clientes");

            entity.HasOne(d => d.FkSetoresNavigation).WithMany(p => p.Treinamento)
                .HasForeignKey(d => d.FkSetores)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Treinamento_SetoresChamados");

            entity.HasOne(d => d.FkSituacaoChamado).WithMany(p => p.Treinamento)
                .HasForeignKey(d => d.FkSituacaoChamadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Treinamento_SituacoesChamados");

            entity.HasOne(d => d.FkTecnico).WithMany(p => p.TreinamentoFkTecnico)
                .HasForeignKey(d => d.FkTecnicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Treinamento_TecnicosSupervisores");

            entity.HasOne(d => d.FkTipoChamado).WithMany(p => p.Treinamento)
                .HasForeignKey(d => d.FkTipoChamadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Treinamento_TiposChamados");
        });

        modelBuilder.Entity<TreinamentoTempo>(entity =>
        {
            entity.HasKey(e => e.PkId).HasName("PK__Treiname__8BEF1566A8CA9504");

            entity.ToTable("TreinamentoTempo", "dbo");

            entity.Property(e => e.PkId).HasColumnName("Pk_Id");
            entity.Property(e => e.FkTecnico).HasColumnName("fk_Tecnico");
            entity.Property(e => e.FkTreinamento).HasColumnName("fk_Treinamento");
            entity.Property(e => e.Tempo).HasColumnName("tempo");

            entity.HasOne(d => d.FkTecnicoNavigation).WithMany(p => p.TreinamentoTempo)
                .HasForeignKey(d => d.FkTecnico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TreinamentoTempo_TecnicosSupervisores");

            entity.HasOne(d => d.FkTreinamentoNavigation).WithMany(p => p.TreinamentoTempo)
                .HasForeignKey(d => d.FkTreinamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TreinamentoTempo_Treinamento");
        });

        modelBuilder.Entity<VwAtendimentos>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_Atendimentos");

            entity.Property(e => e.Categoria).HasMaxLength(150);
            entity.Property(e => e.DataAbertura).HasColumnType("datetime");
            entity.Property(e => e.DataAtendimento).HasColumnType("datetime");
            entity.Property(e => e.DataFechamento).HasColumnType("datetime");
            entity.Property(e => e.Situacao).HasMaxLength(100);
            entity.Property(e => e.SubCategoria).HasMaxLength(150);
            entity.Property(e => e.Tecnico).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
