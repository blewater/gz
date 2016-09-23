﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace gzDAL.Models
{
    public class DynamicPage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100), Index("DynamicPage_Code", IsUnique = true)]
        public string Code { get; set; }
        
        [Required]
        public string ThumbImageUrl { get; set; }
        [Required]
        public string ThumbTitle { get; set; }
        [Required]
        public string ThumbText { get; set; }

        [Required]
        public bool Live { get; set; }

        [Required]
        public DateTime LiveFrom { get; set; }

        [Required]
        public DateTime LiveTo { get; set; }

        [Required]
        [AllowHtml]
        public string Html { get; set; }
    }
}
