using System.ComponentModel.DataAnnotations;

namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinitionAdd
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsOpenData { get; set; }
        [Required]
        public List<ObjectDefinitionPropertyAdd> Properties { get; set; }

    }
}
