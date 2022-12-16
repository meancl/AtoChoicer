using Microsoft.EntityFrameworkCore;
using RequestBasicInfoProject.DB.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestBasicInfoProject
{
    public class MJTradierContext : DbContext
    {
        public DbSet<BasicInfoReq> basicInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=database-2.clmg3ftdxi2a.ap-northeast-2.rds.amazonaws.com;database=MJTradierDB;user=sbe03253;password=jin94099");
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
