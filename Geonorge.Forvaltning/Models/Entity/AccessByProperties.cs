using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Geonorge.Forvaltning.Models.Entity
{
    public class AccessByProperties
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public List<string>? Contributors { get; set; }
        public string Organization { get; set; }

        public virtual ForvaltningsObjektPropertiesMetadata ForvaltningsObjektPropertiesMetadata { get; set; }
    }
}

//Todo enable RLS and create policy:

//CREATE POLICY "AccessByPropertiesContributors" ON "public"."AccessByProperties"

//AS PERMISSIVE FOR SELECT

//TO public

//USING (
//(EXISTS(SELECT "AccessByProperties"."Contributors"
//   FROM users
//  WHERE(users.organization = ANY("AccessByProperties"."Contributors"))))
//)

//CREATE POLICY "AccessByPropertiesOrganization" ON "public"."AccessByProperties"

//AS PERMISSIVE FOR SELECT

//TO public

//USING (
//(EXISTS(SELECT "AccessByProperties"."Organization"
//   FROM users
//  WHERE(users.organization = AccessByProperties"."Organization")))
//)