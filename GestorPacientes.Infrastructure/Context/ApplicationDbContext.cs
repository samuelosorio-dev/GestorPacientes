using GestorPacientes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Infrastructure.Context
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.Property(p => p.NumeroDocumento)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(p => p.NombreCompleto)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(p => p.FechaNacimiento)
                    .IsRequired()
                    .HasColumnType("date");

            });
        }

        public DbSet<Paciente> Pacientes { get; set; }
    }
}
