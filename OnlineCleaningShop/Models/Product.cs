using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineCleaningShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele produsului este obligatoriu")]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Pretul produsului este obligatoriu")]
        public double Price { get; set; }
        public int Stock { get; set; }

        //ratingul din produs
        public double? Score { get; set; }

        public int? CategoryId { get; set; }

        //PASUL 6
        //cheie externa - un articol este postat de catre un utilizator
        public string? UserId { get; set; }

        //calea imaginii
        [Required(ErrorMessage = "Imaginea produsului este obligatorie")]
        public string Image { get; set; }
        public virtual Category? Category { get; set; }

        //PASUL 6
        //prop virtuala - un produs este postat de catre un user
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}
