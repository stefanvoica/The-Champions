using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCleaningShop.Models
{
    public enum RequestStatus { Pending, Approved, Rejected }

    public class ProductRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public Product Product { get; set; }
    }
}
