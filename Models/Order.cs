using System.ComponentModel.DataAnnotations;

namespace OnlineCleaningShop.Models
{
    public enum DeliveryMethod
    {
        WarehousePickup,
        Easybox,
        Courier
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Introduceti numele cosului")]
        public string Name { get; set; }

        public string? UserId { get; set; }
        public bool IsPaid { get; set; } = false;
        public string? TransactionId { get; set; }

        // NEW FIELDS
        [Required(ErrorMessage = "Selectati metoda de livrare")]

        public DeliveryMethod? DeliveryMethod { get; set; }
        public string? DeliveryAddress { get; set; }        // for Courier
        public string? EasyboxLockerId { get; set; }        // for Easybox

        public double TotalInitial { get; set; } = 0;
        public decimal DeliveryFee { get; set; } = 0;
        public string? PromoCode { get; set; }
        public decimal? TotalWithDiscount { get; set; }
        public decimal Total { get; set; } = 0;
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }

}
