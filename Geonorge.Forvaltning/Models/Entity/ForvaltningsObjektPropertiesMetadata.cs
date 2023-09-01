﻿using System.ComponentModel.DataAnnotations;
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

        public virtual ForvaltningsObjektMetadata ForvaltningsObjektMetadata { get; set; }
    }
}