namespace Geonorge.Forvaltning.Models.Api
{
    public class ObjectAccess
    {
        public int objekt { get; set; }
        public List<AccessByProperty> AccessByProperties { get; set; }
        public List<string>? Contributors { get; set; }
        public List<string>? Viewers { get; set; }
    }

    public class AccessByProperty
    {
        public int PropertyId { get; set; }
        public string Value { get; set; }
        public List<string>? Contributors { get; set; }
    }
}