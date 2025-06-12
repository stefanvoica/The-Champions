using Braintree;

namespace OnlineCleaningShop.Services
{
    public interface IBraintreeService
    {
        string GenerateClientToken();
        Result<Transaction> ProcessPayment(string nonce, decimal amount);
    }
}
