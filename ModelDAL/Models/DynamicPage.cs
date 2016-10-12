using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace gzDAL.Models
{
    public class DynamicPageTemplate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, AllowHtml]
        public string Html { get; set; }
    }

    public class DynamicPage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100), Index("DynamicPage_Code", IsUnique = true)]
        public string Code { get; set; }

        [Required]
        public bool UseInPromoList { get; set; }

        public string ThumbImageUrl { get; set; }
        public string ThumbTitle { get; set; }
        public string ThumbText { get; set; }

        [Required]
        public bool Live { get; set; }

        [Required]
        public DateTime LiveFrom { get; set; }

        [Required]
        public DateTime LiveTo { get; set; }

        [Required, AllowHtml]
        public string Html { get; set; }

        [Required, ForeignKey("DynamicPageTemplate")]
        public int DynamicPageTemplateId { get; set; }

        public DynamicPageTemplate DynamicPageTemplate { get; set; }

        [Required]
        public DateTime Updated { get; set; }

        [Required, DefaultValue(false)]
        public bool Deleted { get; set; }
    }


    public class DynamicPageData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, ForeignKey("DynamicPage")]
        public int DynamicPageId { get; set; }
        public DynamicPage DynamicPage { get; set; }

        [Required]
        public string DataName { get; set; }
        [Required]
        public string DataType { get; set; }
        [Required]
        public string DataValue { get; set; }
    }
}
