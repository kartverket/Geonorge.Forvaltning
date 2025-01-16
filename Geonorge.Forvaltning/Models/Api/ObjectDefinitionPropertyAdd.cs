namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectDefinitionPropertyAdd
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool Hidden { get; set; } = false;
        public List<string>? AllowedValues { get; set; }
    }
}
