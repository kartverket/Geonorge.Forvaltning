using System.ComponentModel.DataAnnotations;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class ForvaltningsObjektMetadata
    {
        public int Id { get; set; }
        [StringLength(255)]
        [Required]
        public string Organization { get; set; }
        [StringLength(255)]
        [Required]
        public string Name { get; set; }
        [StringLength(31)]
        [Required]
        public string TableName { get; set; }
        public virtual List<ForvaltningsObjektPropertiesMetadata>? ForvaltningsObjektPropertiesMetadata { get; set; }
    }
}