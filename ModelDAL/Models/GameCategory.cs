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

        [Required]
        public string Title { get; set; }

        [Required]
        public string GameSlugs { get; set; }

        [Required]
        public DataType Updated { get; set; }
    }
}