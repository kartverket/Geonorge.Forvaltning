using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class ForvaltningsObjektPropertiesMetadata
    {
        public int Id { get; set; }
        [StringLength(255)]
        [Required]
        public string Name { get; set; }
        [StringLength(255)]
        [Required]
        public string DataType { get; set; }
        [StringLength(31)]
        [Required]
        public string ColumnName { get; set; }
        [StringLength(255)]
        [Required]
        public string OrganizationNumber { get; set; }
        public List<string>? Contributors { get; set; }

        public List<string>? AllowedValues { get; set; }

        public virtual ForvaltningsObjektMetadata ForvaltningsObjektMetadata { get; set; }

        public virtual List<AccessByProperties>? AccessByProperties { get; set; }
    }
}