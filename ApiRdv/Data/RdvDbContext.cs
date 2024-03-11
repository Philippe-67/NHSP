using ApiRdv.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ApiRdv.Data
{
    public class RdvDbContext : DbContext
    {
        public RdvDbContext(DbContextOptions<RdvDbContext> options) : base(options)
        {
        }

        public DbSet<Rdv> Rdvs { get; set; }

               

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
    }
}
