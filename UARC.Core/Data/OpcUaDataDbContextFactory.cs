using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Uarc.Core.Data
{
    public class OpcUaDataDbContextFactory : IDesignTimeDbContextFactory<OpcUaDataDbContext>
    {
        private string connectionstring = "Database";

        public string ConnectionString
        {
            //get { return connectionstring; }
            set { connectionstring = value; }
        }

        public DbContextOptionsBuilder<OpcUaDataDbContext> OptionsBuilder
        {
            get { return optionsBuilder; }
            set { optionsBuilder = value; }
        }

        public OpcUaDataDbContext CreateDbContext(string[] args)
        {
            optionsBuilder = new DbContextOptionsBuilder<OpcUaDataDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);

            return new OpcUaDataDbContext(optionsBuilder.Options);
        }

        private DbContextOptionsBuilder<OpcUaDataDbContext> optionsBuilder;
    }
}
