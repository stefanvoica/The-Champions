using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCleaningShop.Models
{
    public class ProductBookmarks
    {
        public class ProductBookmark
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            // cheie primara compusa (Id, ProductId, BookmarkId)
            public int Id { get; set; }
            public int? ProductId { get; set; }
            public int? BookmarkId { get; set; }

            public virtual Product? Product { get; set; }
            public virtual Bookmark? Bookmark { get; set; }

            public DateTime BookmarkDate { get; set; }
        }
    }
}
