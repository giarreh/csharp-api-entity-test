﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace workshop.wwwapi.Models
{
    //TODO: decorate class/columns accordingly    
    [Table("Patients")]
    public class Patient
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("fullname")]
        public string FullName { get; set; }
    }
}
