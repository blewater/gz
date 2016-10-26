using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models
{
    public class GameCategory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required, MaxLength(100), Index("GameCategory_Code", IsUnique = true)]
        public string Code { get; set; }

        [Required, DefaultValue(false)]
        public bool IsMobile { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required, MaxLength(2048)]
        public string GameSlugs { get; set; }

        [Required]
        public DateTime Updated { get; set; }
    }
}