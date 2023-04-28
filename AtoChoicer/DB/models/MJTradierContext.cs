using Microsoft.EntityFrameworkCore;
using AtoChoicer.DB.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtoChoicer
{
    public class MJTradierContext : DbContext
    {
        public DbSet<BasicInfoReq> basicInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=221.149.119.60;port=2023;database=MJTradierDB;user=meancl;password=1234");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BasicInfoReq>(entity =>
            {
                entity.HasKey(k => new { k.생성시간, k.종목코드});
            });
        }
    }
}
