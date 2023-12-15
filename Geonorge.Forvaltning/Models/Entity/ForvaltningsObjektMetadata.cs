using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class ForvaltningsObjektMetadata
    {
        public int Id { get; set; }
        [StringLength(255)]
        [Required]
        public string Organization { get; set; }
        public List<string>? Contributors { get; set; }
        [StringLength(255)]
        [Required]
        public string Name { get; set; }
        [StringLength(2000)]
        public string? Description { get; set; }
        public bool IsOpenData { get; set; }
        public int? srid { get; set; }
        [StringLength(31)]
        [Required]
        public string TableName { get; set; }
        public virtual List<ForvaltningsObjektPropertiesMetadata>? ForvaltningsObjektPropertiesMetadata { get; set; }
    }
}

//Todo enable RLS and create policy:

//CREATE POLICY "Metadata" ON "public"."ForvaltningsObjektMetadata"

//AS PERMISSIVE FOR ALL

//TO public

//USING ((EXISTS (SELECT* FROM users WHERE (users.organization = "ForvaltningsObjektMetadata"."Organization"))))

//WITH CHECK((EXISTS (SELECT* FROM users WHERE (users.organization = "ForvaltningsObjektMetadata"."Organization"))))


//CREATE POLICY "MetadataContributor" ON "public"."ForvaltningsObjektMetadata"

//AS PERMISSIVE FOR SELECT

//TO public

//USING ((EXISTS (SELECT* FROM users WHERE (users.organization = ANY("ForvaltningsObjektMetadata"."Contributors")))))

//CREATE POLICY "MetadataAccessByProperties" ON "public"."ForvaltningsObjektMetadata"

//AS PERMISSIVE FOR ALL

//TO public

//USING (
//("Id" IN(SELECT "ForvaltningsObjektPropertiesMetadata"."ForvaltningsObjektMetadataId"
//   FROM "ForvaltningsObjektPropertiesMetadata",
//    "AccessByProperties",
//    users
//  WHERE(("ForvaltningsObjektPropertiesMetadata"."Id" = "AccessByProperties"."ForvaltningsObjektPropertiesMetadataId") AND(users.organization = ANY("AccessByProperties"."Contributors")))))
//)