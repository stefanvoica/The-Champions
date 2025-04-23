namespace OnlineCleaningShop.Models
{
    public class ProductCommentViewModel
    {
        public string Title { get; set; }
        public List<Comment> ListOfComments { get; set; }
        public string CommentContent { get; set; }
        public int? ProductId { get; set; }
        public decimal? Rating { get; set; }

        public virtual Product? Product { get; set; }
    }
}
