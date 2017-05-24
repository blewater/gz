using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models
{
    public enum CarouselActionType
    {
        Url,
        Page,
        Game
    }


    public class CarouselEntry
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required, MaxLength(100), Index("CarouselEntry_Code", IsUnique = true)]
        public string Code { get; set; }

        [Required, DefaultValue(false)]
        public bool IsMobile { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }
        [Required, MaxLength(255)]
        public string SubTitle { get; set; }
        
        [Required, MaxLength(255)]
        public string ActionText { get; set; }

        [Required, MaxLength(255)]
        public string ActionUrl { get; set; }

        [Required]
        public CarouselActionType ActionType { get; set; }
        
        [MaxLength(255)]
        public string BackgroundImageUrl { get; set; }

        [Required]
        public bool Live { get; set; }

        [Required]
        public DateTime LiveFrom { get; set; }

        [Required]
        public DateTime LiveTo { get; set; }

        [Required]
        public DateTime Updated { get; set; }

        [Required, DefaultValue(false)]
        public bool Deleted { get; set; }
    }
}