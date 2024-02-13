using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class ForvaltningsObjektPropertiesMetadata
    {
        public int Id { get; set; }
        [StringLength(255)]
        [Required]
        public string Name { get; set; }
        [StringLength(255)]
        [Required]
        public string DataType { get; set; }
        [StringLength(31)]
        [Required]
        public string ColumnName { get; set; }
        [StringLength(255)]
        [Required]
        public string OrganizationNumber { get; set; }
        public List<string>? Contributors { get; set; }
        public List<string>? AllowedValues { get; set; }
        public int ForvaltningsObjektMetadataId { get; set; }
        public virtual ForvaltningsObjektMetadata ForvaltningsObjektMetadata { get; set; }
        public virtual List<AccessByProperties>? AccessByProperties { get; set; }
    }
}

//Todo enable RLS and create policy:

//CREATE POLICY "MetadataProperties" ON "public"."ForvaltningsObjektPropertiesMetadata"

//AS PERMISSIVE FOR ALL

//TO public

//USING (

//(EXISTS(SELECT users.id,
//    users.created_at,
//    users.email,
//    users.organization,
//    users.editor,
//    users.role
//   FROM users
//  WHERE(users.organization = ("ForvaltningsObjektPropertiesMetadata"."OrganizationNumber")::text)))
//WITH CHECK(
//(EXISTS(SELECT users.id,
//    users.created_at,
//    users.email,
//    users.organization,
//    users.editor,
//    users.role
//   FROM users
//  WHERE(users.organization = ("ForvaltningsObjektPropertiesMetadata"."OrganizationNumber")::text)))
//)
//)


//CREATE POLICY "MetadataPropertiesContributor" ON "public"."ForvaltningsObjektPropertiesMetadata"

//AS PERMISSIVE FOR SELECT

//TO public

//USING (
//(EXISTS(SELECT users.id,
//    users.created_at,
//    users.email,
//    users.organization,
//    users.editor,
//    users.role
//   FROM users
//  WHERE(users.organization = ANY("ForvaltningsObjektPropertiesMetadata"."Contributors"))))
//)