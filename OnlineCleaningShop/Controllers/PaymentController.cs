using Braintree;
using Microsoft.AspNetCore.Mvc;
using OnlineCleaningShop.Services;
using OnlineCleaningShop.Data;
using Microsoft.EntityFrameworkCore;

namespace OnlineCleaningShop.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IBraintreeService _braintreeService;
        private readonly ApplicationDbContext _db;

        public PaymentController(IBraintreeService braintreeService, ApplicationDbContext db)
        {
            _braintreeService = braintreeService;
            _db = db;
        }

        public IActionResult Index(int orderId)
        {
            var clientToken = _braintreeService.GenerateClientToken();
            ViewBag.ClientToken = clientToken;
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(string payment_method_nonce, int orderId)
        {
            //Retrieve full order entity with details and product prices
            var order = _db.Orders.Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // Calculate total
            var total = order.OrderDetails.Sum(od => od.Product.Price * od.Quantity);

            var result = _braintreeService.ProcessPayment(payment_method_nonce, (decimal)total);

            if (result.IsSuccess())
            {
                //Mark as paid
                order.IsPaid = true;
                order.TransactionId = result.Target.Id;

                _db.SaveChanges();

                return RedirectToAction("Success");
            }
            else
            {
                ViewBag.Message = "Payment failed: " + result.Message;
                return View("Error");
            }
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
