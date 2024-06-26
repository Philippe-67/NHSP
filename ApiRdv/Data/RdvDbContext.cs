﻿using ApiRdv.Models;
using Microsoft.EntityFrameworkCore;

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
