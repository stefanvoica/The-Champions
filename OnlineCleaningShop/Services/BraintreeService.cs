using Braintree;
using Microsoft.Extensions.Configuration;
using System;

namespace OnlineCleaningShop.Services
{
    public class BraintreeService : IBraintreeService
    {
        private readonly IBraintreeGateway _gateway;

        public BraintreeService(IConfiguration config)
        {
            var merchantId = config["Braintree:MerchantId"];
            var publicKey = config["Braintree:PublicKey"];
            var privateKey = config["Braintree:PrivateKey"];

            // Protecție: aruncă excepție dacă ceva lipsește
            if (string.IsNullOrWhiteSpace(merchantId) ||
                string.IsNullOrWhiteSpace(publicKey) ||
                string.IsNullOrWhiteSpace(privateKey))
            {
                throw new ArgumentException("Braintree credentials are missing from configuration.");
            }

            // Debug temporar (opțional): verifici în Output Window dacă valorile sunt citite corect
            Console.WriteLine("Braintree config loaded:");
            Console.WriteLine($"MerchantId: {merchantId}");
            Console.WriteLine($"PublicKey: {publicKey}");

            _gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                MerchantId = merchantId,
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
        }

        public string GenerateClientToken()
        {
            return _gateway.ClientToken.Generate();
        }

        public Result<Transaction> ProcessPayment(string nonce, decimal amount)
        {
            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodNonce = nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            return _gateway.Transaction.Sale(request);
        }
    }
}
