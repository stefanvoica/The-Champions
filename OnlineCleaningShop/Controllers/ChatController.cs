using Microsoft.AspNetCore.Mvc;
using OnlineCleaningShop.Services;
using System.Threading.Tasks;

namespace OnlineCleaningShop.Controllers
{
    public class ChatController : Controller
    {
        private readonly GeminiService _geminiService;

        public ChatController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                ViewBag.Error = "Te rog introdu un mesaj.";
                return View("Index");
            }

            var response = await _geminiService.AskGemini(message);
            ViewBag.UserMessage = message;
            ViewBag.AIResponse = response;

            return View("Index");
        }

        [HttpPost]
        public async Task<JsonResult> AskAjax([FromForm] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, error = "Mesajul este gol." });

            var response = await _geminiService.AskGemini(message);
            return Json(new { success = true, answer = response });
        }
    }
}
