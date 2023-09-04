using System.ComponentModel.DataAnnotations;

namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinition
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public List<ObjectDefinitionProperty> Properties { get; set; }

    }
}
