using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCleaningShop.Models
{
    public class ApplicationUser: IdentityUser
    {
        //PASUL 6: USERI SI ROLURI
        //un user poate posta mai multe review-uri
        public virtual ICollection<Review>? Reviews { get; set; }

        //un user poate posta mai multe produse
        public virtual ICollection<Product>? Products { get; set; }

        //un user poate plasa mai multe comenzi; in curand!

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
