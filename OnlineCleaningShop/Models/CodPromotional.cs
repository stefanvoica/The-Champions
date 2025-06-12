using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCleaningShop.Models
{
    public class CodPromotional
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nume { get; set; }

        [Range(0.01, 1.0, ErrorMessage = "Reducerea trebuie să fie între 1% și 100%")]
        public decimal ProcentReducere { get; set; } // ex: 0.15 = 15%
    }
}

