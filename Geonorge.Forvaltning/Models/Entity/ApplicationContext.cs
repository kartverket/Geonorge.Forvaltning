using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class ApplicationContext : DbContext
    {
        private readonly DbTestConfiguration _config;

        public ApplicationContext(DbContextOptions<ApplicationContext> options, IOptions<DbTestConfiguration> config)
                : base(options)
        {
            _config = config.Value;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            
            options.UseNpgsql(_config.ConnectionString);
        }
        public DbSet<ForvaltningsObjektMetadata> ForvaltningsObjektMetadata { get; set; }
        public DbSet<ForvaltningsObjektPropertiesMetadata> ForvaltningsObjektPropertiesMetadata { get; set; }
    }
}
