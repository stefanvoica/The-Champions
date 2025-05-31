using System.ComponentModel.DataAnnotations;

namespace OnlineCleaningShop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Introduceti numele cosului")]
        public string Name { get; set; }
        public string? UserId { get; set; }
        public bool IsPaid { get; set; } = false;
        public string? TransactionId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
