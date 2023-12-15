namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinitionPropertyEdit
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public List<string>? AllowedValues { get; set; }
    }
}
