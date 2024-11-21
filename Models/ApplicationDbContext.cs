using EdiRetrieval.Models;
using Microsoft.EntityFrameworkCore;

namespace EdiRetrieval.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Register> Registers { get; set; }
        public DbSet<CosmosItem> CosmosItems {get;set;}
    }
}
