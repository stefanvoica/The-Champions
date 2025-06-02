using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnlineCleaningShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LockersController : ControllerBase
    {
        private readonly HttpClient _client;

        public LockersController(HttpClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> GetLockers()
        {
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_SAMEDAY_API_TOKEN");

            var response = await _client.GetAsync("https://api.sameday.ro/lockers");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Could not fetch locker data");

            var lockersJson = await response.Content.ReadAsStringAsync();
            return Content(lockersJson, "application/json");
        }
    }
}
