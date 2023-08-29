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
    }
}