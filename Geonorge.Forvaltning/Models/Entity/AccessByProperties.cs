using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class AccessByProperties
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public List<string>? Contributors { get; set; }

        public virtual ForvaltningsObjektPropertiesMetadata ForvaltningsObjektPropertiesMetadata { get; set; }
    }
}