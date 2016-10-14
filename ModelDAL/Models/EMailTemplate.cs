using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace gzDAL.Models
{
    public class EmailTemplate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100), Index("EmailTemplate_Code", IsUnique = true)]
        public string Code { get; set; }

        [Required, MaxLength(1024)]
        public string Subject { get; set; }

        [Required]
        [AllowHtml]
        public string Body { get; set; }

        [Required]
        public DateTime Updated { get; set; }
    }
}