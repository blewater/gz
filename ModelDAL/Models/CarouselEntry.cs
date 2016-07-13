﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models
{
    public class CarouselEntry
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100), Index("CarouselEntry_Code", IsUnique = true)]
        public string Code { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string SubTitle { get; set; }
        [Required]
        public string ActionUrl { get; set; }
        [Required]
        public string BackroudImageUrl { get; set; }

        [Required]
        public bool Live { get; set; }

        [Required]
        public DateTime LiveFrom { get; set; }

        [Required]
        public DateTime LiveTo { get; set; }
    }
}