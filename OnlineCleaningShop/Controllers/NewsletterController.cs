using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineCleaningShop.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public NewsletterController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var subscribers = await _context.NewsletterSubscribers
                .OrderByDescending(s => s.SubscribedAt)
                .ToListAsync();

            return View(subscribers);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string email)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(email))
                return RedirectToAction("Index", "Home");

            if (_context.NewsletterSubscribers.Any(x => x.Email == email))
            {
                TempData["Error"] = "Ești deja abonat!";
                return RedirectToAction("Index", "Home");
            }

            var subscriber = new NewsletterSubscriber { Email = email };
            _context.NewsletterSubscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            // Trimite email de bun venit
            _emailService.SendWelcomeEmail(email);

            TempData["Message"] = "Te-ai abonat cu succes!";
            return RedirectToAction("Index", "Home");
        }

    }
}
