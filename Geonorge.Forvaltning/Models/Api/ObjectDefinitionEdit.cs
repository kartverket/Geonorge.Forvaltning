using System.ComponentModel.DataAnnotations;

namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinitionEdit
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public List<ObjectDefinitionPropertyEdit> Properties { get; set; }

    }
}
