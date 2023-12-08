using System.ComponentModel.DataAnnotations;

namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinitionEdit
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsOpenData { get; set; }
        [Obsolete("Use in datatype ObjectAccess")]
        public List<string>? Contributors { get; set; }
        [Required]
        public List<ObjectDefinitionPropertyEdit> Properties { get; set; }

    }
}
