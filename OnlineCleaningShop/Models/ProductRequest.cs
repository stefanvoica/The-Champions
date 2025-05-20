using OnlineCleaningShop.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineCleaningShop.Models
{
    public class ProductRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Status { get; set; } = "Pending";

        public Product Product { get; set; }
    }
}
