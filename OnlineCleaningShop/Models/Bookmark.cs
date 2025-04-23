using System.ComponentModel.DataAnnotations;
using static OnlineCleaningShop.Models.ProductBookmarks;

namespace OnlineCleaningShop.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Numele colectiei este obligatoriu")]
        public string Name { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<ProductBookmark>? ProductBookmarks { get; set; }
    }
}
