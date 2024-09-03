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

    public virtual DbSet<Clientes> Clientes { get; set; }

    public virtual DbSet<Contratos> Contratos { get; set; }

    public virtual DbSet<Licencas> Licencas { get; set; }

    public virtual DbSet<PlataformasContratos> PlataformasContratos { get; set; }

    public virtual DbSet<SetoresChamados> SetoresChamados { get; set; }

    public virtual DbSet<SituacoesChamados> SituacoesChamados { get; set; }

    public virtual DbSet<SubcategoriasSetores> SubcategoriasSetores { get; set; }

    public virtual DbSet<TecnicosSupervisores> TecnicosSupervisores { get; set; }

    public virtual DbSet<TiposChamados> TiposChamados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("helpdesk")
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Atendimentos>(entity =>
        {
            entity.HasKey(e => e.AtendimentoId).HasName("PK__Atendime__FD23D5491E91D5CB");

            entity.ToTable("Atendimentos", "dbo");

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
            entity.Property(e => e.FkClienteId).HasColumnName("Fk_ClienteId");
            entity.Property(e => e.FkSituacaoChamadoId).HasColumnName("Fk_SituacaoChamadoId");
            entity.Property(e => e.FkTecnicoId).HasColumnName("Fk_TecnicoId");
            entity.Property(e => e.FkTipoChamadoId).HasColumnName("Fk_TipoChamadoId");
            entity.Property(e => e.TelefoneContato).HasMaxLength(20);

            entity.HasOne(d => d.FkCliente).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chamados__Client__6C190EBB");

            entity.HasOne(d => d.FkSituacaoChamado).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkSituacaoChamadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chamados__Situac__6EF57B66");

            entity.HasOne(d => d.FkTecnico).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkTecnicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chamados__Tecnic__6E01572D");

            entity.HasOne(d => d.FkTipoChamado).WithMany(p => p.Chamados)
                .HasForeignKey(d => d.FkTipoChamadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chamados__TipoCh__6D0D32F4");
        });

        modelBuilder.Entity<Clientes>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__Clientes__71ABD08795878164");

            entity.ToTable("Clientes", "dbo");

            entity.Property(e => e.NomeCliente).HasMaxLength(100);
            entity.Property(e => e.Setor).HasMaxLength(50);
            entity.Property(e => e.TelefoneCliente).HasMaxLength(20);
        });

        modelBuilder.Entity<Contratos>(entity =>
        {
            entity.HasKey(e => e.ContratoId).HasName("PK__Contrato__B238E9730AEF188D");

            entity.ToTable("Contratos", "dbo");

            entity.HasIndex(e => e.IdContrato, "UQ__Contrato__567F8F7021D861FD").IsUnique();

            entity.Property(e => e.DescricaoContrato).HasMaxLength(255);
            entity.Property(e => e.FkClienteId).HasColumnName("Fk_ClienteId");
            entity.Property(e => e.FkPlataformaId).HasColumnName("Fk_PlataformaId");
            entity.Property(e => e.IdContrato)
                .HasMaxLength(50)
                .HasColumnName("ID_CONTRATO");
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

        modelBuilder.Entity<Licencas>(entity =>
        {
            entity.HasKey(e => e.LicencaId).HasName("PK__Licencas__28C8BC25B851726F");

            entity.ToTable("Licencas", "dbo");

            entity.HasIndex(e => e.NumeroSerie, "UQ__Licencas__C5455177A4669547").IsUnique();

            entity.Property(e => e.Descricao).HasMaxLength(255);
            entity.Property(e => e.FkContratoId).HasColumnName("Fk_ContratoId");
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

            entity.Property(e => e.Descricao).HasMaxLength(255);
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

        modelBuilder.Entity<SubcategoriasSetores>(entity =>
        {
            entity.HasKey(e => e.SubcategoriaId).HasName("PK__Subcateg__2FEBBB620E97111D");

            entity.ToTable("SubcategoriasSetores", "dbo");

            entity.Property(e => e.FkSetorId).HasColumnName("Fk_SetorId");
            entity.Property(e => e.NomeSubcategoria).HasMaxLength(100);

            entity.HasOne(d => d.FkSetor).WithMany(p => p.SubcategoriasSetores)
                .HasForeignKey(d => d.FkSetorId)
                .HasConstraintName("FK__Subcatego__Setor__59FA5E80");
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

        modelBuilder.Entity<TiposChamados>(entity =>
        {
            entity.HasKey(e => e.TipoChamadoId).HasName("PK__TiposCha__0A7535DCF0A7FCDD");

            entity.ToTable("TiposChamados", "dbo");

            entity.Property(e => e.DescricaoTipoChamado).HasMaxLength(150);
            entity.Property(e => e.Prioridade).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
