using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class ApplicationContext : DbContext
    {
        private readonly DbConfiguration _config;

        public ApplicationContext(DbContextOptions<ApplicationContext> options, IOptions<DbConfiguration> config)
                : base(options)
        {
            _config = config.Value;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            
            options.UseNpgsql(_config.ForvaltningApiDatabase);
        }
        public DbSet<ForvaltningsObjektMetadata> ForvaltningsObjektMetadata { get; set; }
        public DbSet<ForvaltningsObjektPropertiesMetadata> ForvaltningsObjektPropertiesMetadata { get; set; }
        public DbSet<AccessByProperties> AccessByProperties { get; set; }
    }
}
