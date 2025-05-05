using OnlineCleaningShop.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCleaningShop.Models
{
    public class OrderDetail
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? OrderId { get; set; }
        [Required(ErrorMessage = "Cantitatea este obligatorie")]
        [Range(1, int.MaxValue, ErrorMessage = "Cantitatea trebuie să fie cel puțin 1")]
        public int Quantity { get; set; } = 1;
        public virtual Product? Product { get; set; }
        public virtual Order? Order { get; set; }
        public DateTime OrderDate { get; set; }

    }
}
