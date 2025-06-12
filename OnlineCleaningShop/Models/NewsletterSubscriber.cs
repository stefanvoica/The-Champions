using System.ComponentModel.DataAnnotations;

namespace OnlineCleaningShop.Models
{
    public class NewsletterSubscriber
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
