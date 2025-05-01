using System.ComponentModel.DataAnnotations;

namespace OnlineCleaningShop.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public string? Text { get; set; }

        [Required(ErrorMessage = "Ratingul este obligatoriu")]
        [Range(1, 5, ErrorMessage = "Ratingul trebuie să fie între 1 și 5")]
        public int Rating { get; set; }

        public DateTime Date { get; set; }

        //cheie externa - un review apartine unui produs
        public int ProductId { get; set; }
        //pasul 6
        //prop virtuala - un review este postat de catre un user
        public string? UserId { get; set; }

        //prop virtuala
        public virtual ApplicationUser? User { get; set; }
        public virtual Product? Product { get; set; }
    }
}
