using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class ApplicationContext : DbContext
    {

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
                : base(options)
        {
        }

        public DbSet<ForvaltningsObjektMetadata> ForvaltningsObjektMetadata { get; set; }
        public DbSet<ForvaltningsObjektPropertiesMetadata> ForvaltningsObjektPropertiesMetadata { get; set; }
    }
}
