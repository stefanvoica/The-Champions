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
        public DeliveryMethod? DeliveryMethod { get; set; }
        public string? DeliveryAddress { get; set; }        // for Courier
        public string? EasyboxLockerId { get; set; }        // for Easybox

        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }

}
