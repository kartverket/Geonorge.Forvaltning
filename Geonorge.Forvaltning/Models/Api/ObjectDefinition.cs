using System.ComponentModel.DataAnnotations;

namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinition
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string TableName { get; set; }
        [Required]
        public List<ObjectDefinitionProperty> Properties { get; set; }

    }
}
