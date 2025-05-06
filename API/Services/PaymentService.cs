using DataAccess.Constants;
using DataAccess.DTOs.CarDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using Net.payOS;
using Net.payOS.Types;

namespace API.Services
{
    public class PaymentService
    {
        private readonly IBalancement _balanceRepo;
        private readonly PayOS _payOS;
        private readonly WccsContext _wccsContext;
        //public PaymentService();
        public PaymentService(IBalancement balancement)
        {

            _balanceRepo = balancement;
            _payOS = new PayOS(Constants.clientId, Constants.apiKey, Constants.checksumKey);
        }

        public async Task<string> CreatePaymentLink(DepositDTO request, int userId)
        {
            var items = new List<ItemData> { new(request.Name, request.Quantity, request.Price) };

            var paymentData = new PaymentData(
                request.OrderId,
                request.TotalPrice,
                request.Description,
                items,
                "https://API20250506035221.azurewebsites.net/Payment/callback",
                "https://wireless-charging-system.azurewebsites.net//wireless-charging/payment/success"
            );

            var paymentResult = await _payOS.createPaymentLink(paymentData);
            var balance = _balanceRepo.GetBalanceByUserId(userId);
            int balanceId = balance.Result.BalanceId;

            await _balanceRepo.AddBalanceTransactionAsync(new BalanceTransaction
            {
                OrderCode = request.OrderId.ToString(),
                Amount = request.TotalPrice,
                Status = "PENDING",
                TransactionDate = DateTime.Now,
                BalanceId = balanceId,
                TransactionType = "DEPOSIT"
            });

            return paymentResult.checkoutUrl;
        }


        public async Task HandlePaymentCallback(int orderCode, string status, int userId)
        {

            var payment = await _balanceRepo.UpdateBalanceTransactionStatusAsync(orderCode.ToString(), status);

            if (status.ToUpper() == "PAID" && payment != null)
            {
                var balance = await _balanceRepo.GetBalanceByUserId(userId);
                if (balance != null)
                {
                    balance.Balance1 += payment.Amount;
                    await _balanceRepo.UpdateBalance(balance);
                }

                var balanceTransaction = await _balanceRepo.GetBalanceTransactionByOrderIdAsync(orderCode.ToString());
                if (balanceTransaction != null)
                {
                    balanceTransaction.Status = status;
                    balanceTransaction.TransactionDate = DateTime.Now;
                    await _balanceRepo.UpdateBalanceTransactionStatusAsync(orderCode.ToString(), status);
                }
            }
        }

        public async Task<BalanceTransaction> GetPaymentStatus(int orderId)
        {
            return await _balanceRepo.GetBalanceTransactionByOrderIdAsync(orderId.ToString());
        }

        public async Task<Balance> GetBalanceByUserId(int userId)
        {
            return await _balanceRepo.GetBalanceByUserId(userId);
        }

        public async Task<List<TransactionDTO>> GetTransactionHistory(int userId, DateTime? start, DateTime? end)
        {
            return await _balanceRepo.GetTransactionHistory(userId, start, end);
        }
    }
}

    
