using Microsoft.EntityFrameworkCore;
using Uarc.Infrastructure.Data.Models;

namespace Uarc.Core.Data
{
    public class OpcUaDataDbContext : DbContext
    {
        public OpcUaDataDbContext(DbContextOptions<OpcUaDataDbContext> options) : base(options)
        { }

        public DbSet<SqlOpcUaServer> OpcUaServer { get; set; }
        public DbSet<SqlOpcUaVariable> OpcUaVariable { get; set; }
        public DbSet<SqlOpcUaWert> OpcUaWert { get; set; }
    }
}
