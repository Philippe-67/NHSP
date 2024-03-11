using ApiPraticien.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPraticien.Data
{
    public class PraticienDbContext : DbContext
    {
        public PraticienDbContext(DbContextOptions<PraticienDbContext> options) : base(options)
        {
        }

        public DbSet<Praticien> Praticiens { get; set; }
        // public DbSet<Agenda> Agendas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration de l'entité Praticien si nécessaire
        }
    }
}
