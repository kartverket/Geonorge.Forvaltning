namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinitionPropertyAdd
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public List<string>? AllowedValues { get; set; }
    }
}
